#!/usr/bin/env node

const { execSync } = require('child_process');
const fs = require('fs');
const path = require('path');

/**
 * Frontend Performance Testing Script
 * This script runs various performance tests and generates reports
 */

class PerformanceTester {
  constructor() {
    this.results = {
      bundleAnalysis: null,
      lighthouseAudit: null,
      buildTime: null,
      bundleSize: null,
      recommendations: []
    };
  }

  async runAllTests() {
    console.log('🚀 Starting Frontend Performance Tests...\n');

    try {
      await this.analyzeBundleSize();
      await this.measureBuildTime();
      await this.runLighthouseAudit();
      await this.analyzeCodeSplitting();
      
      this.generateReport();
      this.generateRecommendations();
      
      console.log('\n✅ Performance testing completed!');
      console.log('📊 Check performance-report.json for detailed results');
      
    } catch (error) {
      console.error('❌ Performance testing failed:', error.message);
      process.exit(1);
    }
  }

  async analyzeBundleSize() {
    console.log('📦 Analyzing bundle size...');
    
    try {
      // Build with stats
      execSync('ng build --configuration production --stats-json', { 
        stdio: 'inherit',
        cwd: process.cwd()
      });

      // Read stats file
      const statsPath = path.join(process.cwd(), 'dist/stride-hr-frontend/stats.json');
      if (fs.existsSync(statsPath)) {
        const stats = JSON.parse(fs.readFileSync(statsPath, 'utf8'));
        
        this.results.bundleAnalysis = {
          totalSize: this.calculateTotalSize(stats),
          chunks: this.analyzeChunks(stats),
          assets: this.analyzeAssets(stats)
        };

        console.log(`   Total bundle size: ${this.formatBytes(this.results.bundleAnalysis.totalSize)}`);
        console.log(`   Number of chunks: ${this.results.bundleAnalysis.chunks.length}`);
      }
    } catch (error) {
      console.warn('   ⚠️  Bundle analysis failed:', error.message);
    }
  }

  async measureBuildTime() {
    console.log('⏱️  Measuring build time...');
    
    try {
      const startTime = Date.now();
      
      execSync('ng build --configuration production', { 
        stdio: 'pipe',
        cwd: process.cwd()
      });
      
      const buildTime = Date.now() - startTime;
      this.results.buildTime = buildTime;
      
      console.log(`   Build completed in: ${buildTime}ms`);
      
      if (buildTime > 60000) {
        this.results.recommendations.push('Build time exceeds 1 minute. Consider optimizing build process.');
      }
    } catch (error) {
      console.warn('   ⚠️  Build time measurement failed:', error.message);
    }
  }

  async runLighthouseAudit() {
    console.log('🔍 Running Lighthouse audit...');
    
    try {
      // Start development server
      const server = this.startDevServer();
      
      // Wait for server to start
      await this.waitForServer('http://localhost:4200');
      
      // Run Lighthouse
      const auditResult = execSync(
        'lighthouse http://localhost:4200 --output json --quiet --chrome-flags="--headless"',
        { encoding: 'utf8' }
      );
      
      const audit = JSON.parse(auditResult);
      
      this.results.lighthouseAudit = {
        performance: audit.lhr.categories.performance.score * 100,
        accessibility: audit.lhr.categories.accessibility.score * 100,
        bestPractices: audit.lhr.categories['best-practices'].score * 100,
        seo: audit.lhr.categories.seo.score * 100,
        pwa: audit.lhr.categories.pwa ? audit.lhr.categories.pwa.score * 100 : null,
        metrics: {
          firstContentfulPaint: audit.lhr.audits['first-contentful-paint'].numericValue,
          largestContentfulPaint: audit.lhr.audits['largest-contentful-paint'].numericValue,
          speedIndex: audit.lhr.audits['speed-index'].numericValue,
          timeToInteractive: audit.lhr.audits['interactive'].numericValue,
          totalBlockingTime: audit.lhr.audits['total-blocking-time'].numericValue,
          cumulativeLayoutShift: audit.lhr.audits['cumulative-layout-shift'].numericValue
        }
      };

      console.log(`   Performance Score: ${this.results.lighthouseAudit.performance}/100`);
      console.log(`   First Contentful Paint: ${this.results.lighthouseAudit.metrics.firstContentfulPaint}ms`);
      console.log(`   Largest Contentful Paint: ${this.results.lighthouseAudit.metrics.largestContentfulPaint}ms`);
      
      // Stop server
      server.kill();
      
    } catch (error) {
      console.warn('   ⚠️  Lighthouse audit failed:', error.message);
    }
  }

  async analyzeCodeSplitting() {
    console.log('🔀 Analyzing code splitting...');
    
    try {
      const distPath = path.join(process.cwd(), 'dist/stride-hr-frontend');
      const files = fs.readdirSync(distPath);
      
      const jsFiles = files.filter(file => file.endsWith('.js'));
      const lazyChunks = jsFiles.filter(file => file.match(/^\d+\./)); // Lazy loaded chunks
      
      console.log(`   Total JS files: ${jsFiles.length}`);
      console.log(`   Lazy loaded chunks: ${lazyChunks.length}`);
      
      if (lazyChunks.length < 3) {
        this.results.recommendations.push('Consider implementing more lazy loading to improve initial load time.');
      }
      
    } catch (error) {
      console.warn('   ⚠️  Code splitting analysis failed:', error.message);
    }
  }

  calculateTotalSize(stats) {
    return stats.assets.reduce((total, asset) => total + asset.size, 0);
  }

  analyzeChunks(stats) {
    return stats.chunks.map(chunk => ({
      id: chunk.id,
      names: chunk.names,
      size: chunk.size,
      files: chunk.files
    }));
  }

  analyzeAssets(stats) {
    return stats.assets
      .sort((a, b) => b.size - a.size)
      .slice(0, 10) // Top 10 largest assets
      .map(asset => ({
        name: asset.name,
        size: asset.size,
        type: this.getAssetType(asset.name)
      }));
  }

  getAssetType(filename) {
    if (filename.endsWith('.js')) return 'JavaScript';
    if (filename.endsWith('.css')) return 'CSS';
    if (filename.match(/\.(png|jpg|jpeg|gif|svg|webp)$/)) return 'Image';
    if (filename.match(/\.(woff|woff2|ttf|otf)$/)) return 'Font';
    return 'Other';
  }

  startDevServer() {
    const { spawn } = require('child_process');
    return spawn('ng', ['serve', '--port=4200'], {
      stdio: 'pipe',
      detached: false
    });
  }

  async waitForServer(url, timeout = 30000) {
    const start = Date.now();
    
    while (Date.now() - start < timeout) {
      try {
        const { execSync } = require('child_process');
        execSync(`curl -s ${url} > /dev/null`, { stdio: 'pipe' });
        return;
      } catch {
        await new Promise(resolve => setTimeout(resolve, 1000));
      }
    }
    
    throw new Error('Server failed to start within timeout');
  }

  generateReport() {
    const report = {
      timestamp: new Date().toISOString(),
      summary: {
        bundleSize: this.results.bundleAnalysis?.totalSize,
        buildTime: this.results.buildTime,
        performanceScore: this.results.lighthouseAudit?.performance,
        recommendations: this.results.recommendations
      },
      details: this.results
    };

    fs.writeFileSync('performance-report.json', JSON.stringify(report, null, 2));
  }

  generateRecommendations() {
    console.log('\n📋 Performance Recommendations:');
    
    // Bundle size recommendations
    if (this.results.bundleAnalysis?.totalSize > 2 * 1024 * 1024) {
      this.results.recommendations.push('Bundle size exceeds 2MB. Consider code splitting and tree shaking.');
    }

    // Performance score recommendations
    if (this.results.lighthouseAudit?.performance < 90) {
      this.results.recommendations.push('Performance score is below 90. Focus on Core Web Vitals optimization.');
    }

    // Lighthouse metrics recommendations
    if (this.results.lighthouseAudit?.metrics.firstContentfulPaint > 1800) {
      this.results.recommendations.push('First Contentful Paint is slow. Optimize critical rendering path.');
    }

    if (this.results.lighthouseAudit?.metrics.largestContentfulPaint > 2500) {
      this.results.recommendations.push('Largest Contentful Paint is slow. Optimize images and implement lazy loading.');
    }

    if (this.results.lighthouseAudit?.metrics.cumulativeLayoutShift > 0.1) {
      this.results.recommendations.push('High Cumulative Layout Shift detected. Set explicit dimensions for images.');
    }

    // Display recommendations
    if (this.results.recommendations.length === 0) {
      console.log('   ✅ No performance issues detected!');
    } else {
      this.results.recommendations.forEach((rec, index) => {
        console.log(`   ${index + 1}. ${rec}`);
      });
    }
  }

  formatBytes(bytes) {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }
}

// Run the performance tests
if (require.main === module) {
  const tester = new PerformanceTester();
  tester.runAllTests().catch(console.error);
}

module.exports = PerformanceTester;