import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, firstValueFrom } from 'rxjs';

export interface AIInsight {
  id: string;
  title: string;
  description: string;
  type: 'trend' | 'anomaly' | 'prediction' | 'recommendation';
  severity: 'low' | 'medium' | 'high';
  confidence: number;
  data: any;
  actionable: boolean;
  createdAt: Date;
}

export interface PredictiveAnalytics {
  turnoverRisk: {
    employeeId: number;
    riskScore: number;
    factors: string[];
    recommendations: string[];
  }[];
  performanceForecasting: {
    departmentId: number;
    predictedScore: number;
    trend: 'improving' | 'declining' | 'stable';
    confidence: number;
  }[];
  workforceOptimization: {
    recommendations: string[];
    potentialSavings: number;
    implementationEffort: 'low' | 'medium' | 'high';
  };
}

export interface SentimentAnalysis {
  overallSentiment: 'positive' | 'neutral' | 'negative';
  sentimentScore: number;
  departmentBreakdown: {
    department: string;
    sentiment: 'positive' | 'neutral' | 'negative';
    score: number;
  }[];
  keyTopics: {
    topic: string;
    sentiment: 'positive' | 'neutral' | 'negative';
    mentions: number;
  }[];
  trends: {
    period: string;
    score: number;
  }[];
}

export interface WorkforceAnalytics {
  headcountTrends: {
    period: string;
    total: number;
    hires: number;
    departures: number;
  }[];
  diversityMetrics: {
    gender: { [key: string]: number };
    age: { [key: string]: number };
    department: { [key: string]: number };
  };
  skillsGapAnalysis: {
    skill: string;
    currentLevel: number;
    requiredLevel: number;
    gap: number;
    priority: 'high' | 'medium' | 'low';
  }[];
}

@Injectable({
  providedIn: 'root'
})
export class AIAnalyticsService {
  private readonly apiUrl = '/api/ai-analytics';

  constructor(private http: HttpClient) {}

  // AI Insights
  async getAIInsights(timeRange?: string): Promise<AIInsight[]> {
    const params = timeRange ? { timeRange } : undefined;
    return firstValueFrom(this.http.get<AIInsight[]>(`${this.apiUrl}/insights`, { params }));
  }

  async generateInsight(dataSource: string, analysisType: string): Promise<AIInsight> {
    return firstValueFrom(this.http.post<AIInsight>(`${this.apiUrl}/insights/generate`, {
      dataSource,
      analysisType
    }));
  }

  async dismissInsight(insightId: string): Promise<void> {
    return firstValueFrom(this.http.delete<void>(`${this.apiUrl}/insights/${insightId}`));
  }

  async implementRecommendation(insightId: string, action: string): Promise<{ success: boolean; message: string }> {
    return firstValueFrom(this.http.post<{ success: boolean; message: string }>(
      `${this.apiUrl}/insights/${insightId}/implement`,
      { action }
    ));
  }

  // Predictive Analytics
  async getPredictiveAnalytics(timeRange?: string): Promise<PredictiveAnalytics> {
    const params = timeRange ? { timeRange } : undefined;
    return firstValueFrom(this.http.get<PredictiveAnalytics>(`${this.apiUrl}/predictive`, { params }));
  }

  async getTurnoverRiskPrediction(employeeId?: number): Promise<any> {
    const params = employeeId ? { employeeId: employeeId.toString() } : undefined;
    return firstValueFrom(this.http.get<any>(`${this.apiUrl}/predictive/turnover-risk`, { params }));
  }

  async getPerformanceForecasting(departmentId?: number): Promise<any> {
    const params = departmentId ? { departmentId: departmentId.toString() } : undefined;
    return firstValueFrom(this.http.get<any>(`${this.apiUrl}/predictive/performance`, { params }));
  }

  async getWorkforceOptimization(): Promise<any> {
    return firstValueFrom(this.http.get<any>(`${this.apiUrl}/predictive/workforce-optimization`));
  }

  // Sentiment Analysis
  async getSentimentAnalysis(timeRange?: string): Promise<SentimentAnalysis> {
    const params = timeRange ? { timeRange } : undefined;
    return firstValueFrom(this.http.get<SentimentAnalysis>(`${this.apiUrl}/sentiment`, { params }));
  }

  async analyzeFeedback(feedbackText: string): Promise<{ sentiment: string; score: number; topics: string[] }> {
    return firstValueFrom(this.http.post<{ sentiment: string; score: number; topics: string[] }>(
      `${this.apiUrl}/sentiment/analyze`,
      { text: feedbackText }
    ));
  }

  async getSentimentTrends(period: string): Promise<any> {
    return firstValueFrom(this.http.get<any>(`${this.apiUrl}/sentiment/trends`, { 
      params: { period } 
    }));
  }

  // Workforce Analytics
  async getWorkforceAnalytics(timeRange?: string): Promise<WorkforceAnalytics> {
    const params = timeRange ? { timeRange } : undefined;
    return firstValueFrom(this.http.get<WorkforceAnalytics>(`${this.apiUrl}/workforce`, { params }));
  }

  async getHeadcountTrends(period: string): Promise<any> {
    return firstValueFrom(this.http.get<any>(`${this.apiUrl}/workforce/headcount-trends`, {
      params: { period }
    }));
  }

  async getDiversityMetrics(): Promise<any> {
    return firstValueFrom(this.http.get<any>(`${this.apiUrl}/workforce/diversity`));
  }

  async getSkillsGapAnalysis(): Promise<any> {
    return firstValueFrom(this.http.get<any>(`${this.apiUrl}/workforce/skills-gap`));
  }

  // Performance Analytics
  async getPerformanceInsights(timeRange?: string): Promise<any> {
    const params = timeRange ? { timeRange } : undefined;
    return firstValueFrom(this.http.get<any>(`${this.apiUrl}/performance/insights`, { params }));
  }

  async getTopPerformers(limit: number = 10): Promise<any> {
    return firstValueFrom(this.http.get<any>(`${this.apiUrl}/performance/top-performers`, {
      params: { limit: limit.toString() }
    }));
  }

  async getPerformanceTrends(employeeId?: number, departmentId?: number): Promise<any> {
    const params: any = {};
    if (employeeId) params.employeeId = employeeId.toString();
    if (departmentId) params.departmentId = departmentId.toString();
    
    return firstValueFrom(this.http.get<any>(`${this.apiUrl}/performance/trends`, { params }));
  }

  // Attendance Analytics
  async getAttendanceInsights(timeRange?: string): Promise<any> {
    const params = timeRange ? { timeRange } : undefined;
    return firstValueFrom(this.http.get<any>(`${this.apiUrl}/attendance/insights`, { params }));
  }

  async getAttendancePatterns(): Promise<any> {
    return firstValueFrom(this.http.get<any>(`${this.apiUrl}/attendance/patterns`));
  }

  async getAttendanceAnomalies(): Promise<any> {
    return firstValueFrom(this.http.get<any>(`${this.apiUrl}/attendance/anomalies`));
  }

  // Project Analytics
  async getProjectInsights(timeRange?: string): Promise<any> {
    const params = timeRange ? { timeRange } : undefined;
    return firstValueFrom(this.http.get<any>(`${this.apiUrl}/projects/insights`, { params }));
  }

  async getProjectRiskAnalysis(): Promise<any> {
    return firstValueFrom(this.http.get<any>(`${this.apiUrl}/projects/risk-analysis`));
  }

  async getResourceUtilization(): Promise<any> {
    return firstValueFrom(this.http.get<any>(`${this.apiUrl}/projects/resource-utilization`));
  }

  // Custom Analytics
  async runCustomAnalysis(config: {
    dataSource: string;
    metrics: string[];
    filters: any;
    analysisType: string;
  }): Promise<any> {
    return firstValueFrom(this.http.post<any>(`${this.apiUrl}/custom/analyze`, config));
  }

  async saveCustomAnalysis(name: string, config: any): Promise<{ id: string; message: string }> {
    return firstValueFrom(this.http.post<{ id: string; message: string }>(
      `${this.apiUrl}/custom/save`,
      { name, config }
    ));
  }

  async getCustomAnalyses(): Promise<any[]> {
    return firstValueFrom(this.http.get<any[]>(`${this.apiUrl}/custom`));
  }

  // Benchmarking
  async getIndustryBenchmarks(industry?: string): Promise<any> {
    const params = industry ? { industry } : undefined;
    return firstValueFrom(this.http.get<any>(`${this.apiUrl}/benchmarks/industry`, { params }));
  }

  async compareToBenchmarks(metrics: string[]): Promise<any> {
    return firstValueFrom(this.http.post<any>(`${this.apiUrl}/benchmarks/compare`, { metrics }));
  }

  // Real-time Analytics
  async getRealtimeMetrics(): Promise<any> {
    return firstValueFrom(this.http.get<any>(`${this.apiUrl}/realtime/metrics`));
  }

  subscribeToRealtimeUpdates(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/realtime/subscribe`);
  }

  // Export and Reporting
  async exportAnalytics(format: 'pdf' | 'excel' | 'csv', config: any): Promise<Blob> {
    return firstValueFrom(this.http.post(`${this.apiUrl}/export`, 
      { format, config }, 
      { responseType: 'blob' }
    ));
  }

  async scheduleAnalyticsReport(config: {
    name: string;
    schedule: string;
    recipients: string[];
    format: string;
    analytics: string[];
  }): Promise<{ success: boolean; scheduleId: string }> {
    return firstValueFrom(this.http.post<{ success: boolean; scheduleId: string }>(
      `${this.apiUrl}/schedule`,
      config
    ));
  }

  // Machine Learning Model Management
  async getModelPerformance(): Promise<any> {
    return firstValueFrom(this.http.get<any>(`${this.apiUrl}/models/performance`));
  }

  async retrainModel(modelType: string): Promise<{ success: boolean; message: string }> {
    return firstValueFrom(this.http.post<{ success: boolean; message: string }>(
      `${this.apiUrl}/models/retrain`,
      { modelType }
    ));
  }

  async getModelPredictions(modelType: string, inputData: any): Promise<any> {
    return firstValueFrom(this.http.post<any>(`${this.apiUrl}/models/predict`, {
      modelType,
      inputData
    }));
  }
}