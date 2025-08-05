/**
 * Browser compatibility configuration and feature detection
 * Defines supported browsers and required features for StrideHR
 */

export interface BrowserSupport {
  name: string;
  minVersion: string;
  features: BrowserFeature[];
  testPriority: 'high' | 'medium' | 'low';
}

export interface BrowserFeature {
  name: string;
  required: boolean;
  testFunction: () => boolean;
  fallback?: string;
}

export class BrowserCompatibilityConfig {
  
  /**
   * Supported browsers with minimum versions
   */
  static readonly SUPPORTED_BROWSERS: BrowserSupport[] = [
    {
      name: 'Chrome',
      minVersion: '90',
      testPriority: 'high',
      features: [
        {
          name: 'ES6 Modules',
          required: true,
          testFunction: () => 'noModule' in HTMLScriptElement.prototype
        },
        {
          name: 'CSS Grid',
          required: true,
          testFunction: () => CSS.supports('display', 'grid')
        },
        {
          name: 'Flexbox',
          required: true,
          testFunction: () => CSS.supports('display', 'flex')
        },
        {
          name: 'Service Workers',
          required: true,
          testFunction: () => 'serviceWorker' in navigator
        },
        {
          name: 'Web Push',
          required: false,
          testFunction: () => 'PushManager' in window
        }
      ]
    },
    {
      name: 'Firefox',
      minVersion: '88',
      testPriority: 'high',
      features: [
        {
          name: 'ES6 Modules',
          required: true,
          testFunction: () => 'noModule' in HTMLScriptElement.prototype
        },
        {
          name: 'CSS Grid',
          required: true,
          testFunction: () => CSS.supports('display', 'grid')
        },
        {
          name: 'Flexbox',
          required: true,
          testFunction: () => CSS.supports('display', 'flex')
        },
        {
          name: 'Service Workers',
          required: true,
          testFunction: () => 'serviceWorker' in navigator
        }
      ]
    },
    {
      name: 'Safari',
      minVersion: '14',
      testPriority: 'medium',
      features: [
        {
          name: 'ES6 Modules',
          required: true,
          testFunction: () => 'noModule' in HTMLScriptElement.prototype
        },
        {
          name: 'CSS Grid',
          required: true,
          testFunction: () => CSS.supports('display', 'grid')
        },
        {
          name: 'Flexbox',
          required: true,
          testFunction: () => CSS.supports('display', 'flex')
        },
        {
          name: 'Service Workers',
          required: false,
          testFunction: () => 'serviceWorker' in navigator,
          fallback: 'Limited PWA functionality'
        }
      ]
    },
    {
      name: 'Edge',
      minVersion: '90',
      testPriority: 'high',
      features: [
        {
          name: 'ES6 Modules',
          required: true,
          testFunction: () => 'noModule' in HTMLScriptElement.prototype
        },
        {
          name: 'CSS Grid',
          required: true,
          testFunction: () => CSS.supports('display', 'grid')
        },
        {
          name: 'Flexbox',
          required: true,
          testFunction: () => CSS.supports('display', 'flex')
        },
        {
          name: 'Service Workers',
          required: true,
          testFunction: () => 'serviceWorker' in navigator
        }
      ]
    }
  ];

  /**
   * Mobile browsers with specific considerations
   */
  static readonly MOBILE_BROWSERS: BrowserSupport[] = [
    {
      name: 'Chrome Mobile',
      minVersion: '90',
      testPriority: 'high',
      features: [
        {
          name: 'Touch Events',
          required: true,
          testFunction: () => 'ontouchstart' in window
        },
        {
          name: 'Viewport Meta',
          required: true,
          testFunction: () => !!document.querySelector('meta[name="viewport"]')
        },
        {
          name: 'PWA Install',
          required: false,
          testFunction: () => 'BeforeInstallPromptEvent' in window
        }
      ]
    },
    {
      name: 'Safari Mobile',
      minVersion: '14',
      testPriority: 'high',
      features: [
        {
          name: 'Touch Events',
          required: true,
          testFunction: () => 'ontouchstart' in window
        },
        {
          name: 'Viewport Meta',
          required: true,
          testFunction: () => !!document.querySelector('meta[name="viewport"]')
        },
        {
          name: 'PWA Install',
          required: false,
          testFunction: () => false, // Safari uses different PWA install method
          fallback: 'Add to Home Screen via Share menu'
        }
      ]
    }
  ];

  /**
   * Core web features required for StrideHR
   */
  static readonly CORE_FEATURES: BrowserFeature[] = [
    {
      name: 'Fetch API',
      required: true,
      testFunction: () => 'fetch' in window,
      fallback: 'XMLHttpRequest polyfill'
    },
    {
      name: 'Promises',
      required: true,
      testFunction: () => 'Promise' in window,
      fallback: 'Promise polyfill'
    },
    {
      name: 'Local Storage',
      required: true,
      testFunction: () => {
        try {
          localStorage.setItem('test', 'test');
          localStorage.removeItem('test');
          return true;
        } catch {
          return false;
        }
      }
    },
    {
      name: 'Session Storage',
      required: true,
      testFunction: () => {
        try {
          sessionStorage.setItem('test', 'test');
          sessionStorage.removeItem('test');
          return true;
        } catch {
          return false;
        }
      }
    },
    {
      name: 'JSON',
      required: true,
      testFunction: () => 'JSON' in window && typeof JSON.parse === 'function'
    },
    {
      name: 'Array Methods',
      required: true,
      testFunction: () => {
        try {
          const testArray = [1, 2, 3];
          return typeof testArray.map === 'function' && 
                 typeof testArray.filter === 'function' && 
                 typeof testArray.reduce === 'function' &&
                 testArray.map(x => x * 2).length === 3;
        } catch {
          return false;
        }
      }
    },
    {
      name: 'Object Methods',
      required: true,
      testFunction: () => {
        try {
          const testObj = { a: 1, b: 2 };
          return typeof Object.keys === 'function' && 
                 typeof Object.assign === 'function' &&
                 Object.keys(testObj).length === 2;
        } catch {
          return false;
        }
      }
    },
    {
      name: 'Date API',
      required: true,
      testFunction: () => {
        try {
          new Date().toISOString();
          return true;
        } catch {
          return false;
        }
      }
    }
  ];

  /**
   * CSS features required for proper styling
   */
  static readonly CSS_FEATURES: BrowserFeature[] = [
    {
      name: 'CSS Custom Properties',
      required: true,
      testFunction: () => CSS.supports('color', 'var(--test)')
    },
    {
      name: 'CSS Transforms',
      required: true,
      testFunction: () => CSS.supports('transform', 'translateX(10px)')
    },
    {
      name: 'CSS Transitions',
      required: true,
      testFunction: () => CSS.supports('transition', 'opacity 0.3s')
    },
    {
      name: 'CSS Media Queries',
      required: true,
      testFunction: () => 'matchMedia' in window
    },
    {
      name: 'CSS Calc',
      required: true,
      testFunction: () => CSS.supports('width', 'calc(100% - 10px)')
    },
    {
      name: 'CSS Viewport Units',
      required: true,
      testFunction: () => CSS.supports('height', '100vh')
    }
  ];

  /**
   * PWA-specific features
   */
  static readonly PWA_FEATURES: BrowserFeature[] = [
    {
      name: 'Service Workers',
      required: true,
      testFunction: () => 'serviceWorker' in navigator
    },
    {
      name: 'Cache API',
      required: true,
      testFunction: () => 'caches' in window
    },
    {
      name: 'Push Manager',
      required: false,
      testFunction: () => 'PushManager' in window
    },
    {
      name: 'Notifications',
      required: false,
      testFunction: () => 'Notification' in window
    },
    {
      name: 'Background Sync',
      required: false,
      testFunction: () => 'serviceWorker' in navigator && 'sync' in window.ServiceWorkerRegistration.prototype
    },
    {
      name: 'Web App Manifest',
      required: true,
      testFunction: () => !!document.querySelector('link[rel="manifest"]')
    }
  ];

  /**
   * Test all browser features and return compatibility report
   */
  static testBrowserCompatibility(): BrowserCompatibilityReport {
    const report: BrowserCompatibilityReport = {
      browser: this.detectBrowser(),
      compatible: true,
      features: {
        core: this.testFeatureSet(this.CORE_FEATURES),
        css: this.testFeatureSet(this.CSS_FEATURES),
        pwa: this.testFeatureSet(this.PWA_FEATURES)
      },
      warnings: [],
      errors: []
    };

    // Check for critical failures
    const coreFailures = report.features.core.filter(f => !f.supported && f.required);
    const cssFailures = report.features.css.filter(f => !f.supported && f.required);

    if (coreFailures.length > 0 || cssFailures.length > 0) {
      report.compatible = false;
      report.errors.push(...coreFailures.map(f => `Missing required feature: ${f.name}`));
      report.errors.push(...cssFailures.map(f => `Missing required CSS feature: ${f.name}`));
    }

    // Check for PWA warnings
    const pwaFailures = report.features.pwa.filter(f => !f.supported && f.required);
    if (pwaFailures.length > 0) {
      report.warnings.push(...pwaFailures.map(f => `PWA feature not available: ${f.name}`));
    }

    return report;
  }

  /**
   * Test a set of features
   */
  private static testFeatureSet(features: BrowserFeature[]): FeatureTestResult[] {
    return features.map(feature => ({
      name: feature.name,
      required: feature.required,
      supported: feature.testFunction(),
      fallback: feature.fallback
    }));
  }

  /**
   * Detect current browser
   */
  private static detectBrowser(): BrowserInfo {
    const userAgent = navigator.userAgent;
    
    if (userAgent.includes('Chrome') && !userAgent.includes('Edg')) {
      return { name: 'Chrome', version: this.extractVersion(userAgent, 'Chrome/') };
    } else if (userAgent.includes('Firefox')) {
      return { name: 'Firefox', version: this.extractVersion(userAgent, 'Firefox/') };
    } else if (userAgent.includes('Safari') && !userAgent.includes('Chrome')) {
      return { name: 'Safari', version: this.extractVersion(userAgent, 'Version/') };
    } else if (userAgent.includes('Edg')) {
      return { name: 'Edge', version: this.extractVersion(userAgent, 'Edg/') };
    } else {
      return { name: 'Unknown', version: 'Unknown' };
    }
  }

  /**
   * Extract version number from user agent string
   */
  private static extractVersion(userAgent: string, versionString: string): string {
    const index = userAgent.indexOf(versionString);
    if (index === -1) return 'Unknown';
    
    const version = userAgent.substring(index + versionString.length);
    const match = version.match(/^(\d+(?:\.\d+)*)/);
    return match ? match[1] : 'Unknown';
  }

  /**
   * Get browser-specific test configuration
   */
  static getBrowserTestConfig(browserName: string): BrowserTestConfig {
    const browser = this.SUPPORTED_BROWSERS.find(b => 
      b.name.toLowerCase() === browserName.toLowerCase()
    );

    if (!browser) {
      throw new Error(`Unsupported browser: ${browserName}`);
    }

    return {
      browser: browser.name,
      features: browser.features,
      testPriority: browser.testPriority,
      requiredFeatures: browser.features.filter(f => f.required),
      optionalFeatures: browser.features.filter(f => !f.required)
    };
  }
}

export interface BrowserCompatibilityReport {
  browser: BrowserInfo;
  compatible: boolean;
  features: {
    core: FeatureTestResult[];
    css: FeatureTestResult[];
    pwa: FeatureTestResult[];
  };
  warnings: string[];
  errors: string[];
}

export interface BrowserInfo {
  name: string;
  version: string;
}

export interface FeatureTestResult {
  name: string;
  required: boolean;
  supported: boolean;
  fallback?: string;
}

export interface BrowserTestConfig {
  browser: string;
  features: BrowserFeature[];
  testPriority: 'high' | 'medium' | 'low';
  requiredFeatures: BrowserFeature[];
  optionalFeatures: BrowserFeature[];
}