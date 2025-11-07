
import apiClient from '@/api/axiosInstance';

// src/modules/ReportForm/api/reportForm.ts
export type CardType = 'revenue' | 'occupancy' | 'heatmap' | 'occupancy_kpi' | 'revenue_kpi' | 'revenue_source' | 'occupancy_source' | 'revenue_prediction' | 'occupancy_prediction';

export interface BaseDraft {
  type: CardType;
  title: string;
  subtitle?: string;
}

export interface RevenueConfig {
  propertyIds: number[];
  startDate: string;
  endDate: string;
  groupBy: 'day' | 'week' | 'month';
  chartType?: 'line' | 'bar' | 'pie';
}

export interface OccupancyConfig {
  propertyIds: number[];
  startDate: string;
  endDate: string;
  groupBy: 'day' | 'week' | 'month';
  chartType: 'line' | 'bar' | 'pie';
}

export interface RevenuePredictionConfig {
  propertyIds: number[];
  forecastDays: number;
  chartType: 'line' | 'bar';
}

export interface OccupancyPredictionConfig {
  propertyIds: number[];
  forecastDays: number;
  chartType: 'line' | 'bar';
}

export interface OccupancyKpiConfig {
    propertyIds: number[];
    lastDays: number;
}

export interface RevenueKpiConfig {
    propertyIds: number[];
    lastDays: number;
}

export interface RevenueSourceConfig {
    propertyIds: number[];
    lastDays: number;
}

export interface OccupancySourceConfig {
    propertyIds: number[];
    lastDays: number;
}

export interface HeatmapConfig {
  propertyIds: string[];
  center: { lat: number; lng: number };
  zoom: number;
}

export type CardConfig = RevenueConfig | OccupancyConfig | HeatmapConfig | OccupancyKpiConfig | RevenueKpiConfig | RevenueSourceConfig | OccupancySourceConfig | RevenuePredictionConfig | OccupancyPredictionConfig;

export interface CardDraft extends BaseDraft {
  config: CardConfig;
}

export interface Card {
  id: string;
  type: CardType;
  title: string;
  subtitle?: string;
  config: CardConfig;
  data: any;
}

// ---- Favorite Report Types ----
export interface FavoriteReport {
    id: number;
    name: string;
}

export interface FavoriteReportDetail extends FavoriteReport {
    content: string;
    createdAt: string;
}

// ---- Favorite Report API ----
export async function getFavoriteReports(): Promise<FavoriteReport[]> {
    try {
        const response = await apiClient.get('/ReportForm/FavoriteReports');
        return response.data;
    } catch (error) {
        console.error('Error fetching favorite reports:', error);
        return [];
    }
}

export async function loadFavoriteReport(id: number): Promise<FavoriteReportDetail | null> {
    try {
        const response = await apiClient.get(`/ReportForm/FavoriteReports/${id}`);
        return response.data;
    } catch (error) {
        console.error(`Error loading favorite report ${id}:`, error);
        return null;
    }
}

export async function saveFavoriteReport(name: string, content: string): Promise<FavoriteReport | null> {
    try {
        const response = await apiClient.post('/ReportForm/FavoriteReports', { name, content });
        return response.data;
    } catch (error) {
        console.error('Error saving favorite report:', error);
        return null;
    }
}

export async function deleteFavoriteReport(id: number): Promise<boolean> {
    try {
        await apiClient.delete(`/ReportForm/FavoriteReports/${id}`);
        return true;
    } catch (error) {
        console.error(`Error deleting favorite report ${id}:`, error);
        return false;
    }
}


// ---- Mock AJAX helpers ----
function delay<T>(val: T, ms = 300): Promise<T> {
  return new Promise(res => setTimeout(() => res(val), ms));
}

export async function createCard(draft: CardDraft): Promise<Card> {
  const data = await fetchCardData(draft.type, draft.config);
  const card: Card = {
    id: cryptoRandomId(),
    type: draft.type,
    title: draft.title,
    subtitle: draft.subtitle,
    config: draft.config,
    data
  };
  return delay(card, 250);
}

export async function updateCard(id: string, draft: CardDraft): Promise<Card> {
  const data = await fetchCardData(draft.type, draft.config);
  const card: Card = {
    id,
    type: draft.type,
    title: draft.title,
    subtitle: draft.subtitle,
    config: draft.config,
    data
  };
  return delay(card, 250);
}

export async function refetchCardData(card: Card): Promise<Card> {
  const data = await fetchCardData(card.type, card.config as any);
  return delay({ ...card, data }, 200);
}

// ---- Data fetching logic ----
export async function fetchCardData(type: CardType, config: CardConfig): Promise<any> {
  if (type === 'revenue') {
    const revenueConfig = config as RevenueConfig;
    const requestDto = {
        RoomIds: revenueConfig.propertyIds,
        StartDate: revenueConfig.startDate,
        EndDate: revenueConfig.endDate,
        GroupBy: revenueConfig.groupBy
    };
    
    try {
        const response = await apiClient.post('/ReportForm/Revenue/GetRevenue', requestDto);
        // The backend returns an array of { Date, Revenue }, wrap it in a 'points' property
        return { points: response.data };
    } catch (error) {
        console.error('Error fetching revenue data:', error);
        // Return empty points on error
        return { points: [] };
    }
  }
  
  if (type === 'occupancy') {
    const occupancyConfig = config as OccupancyConfig;
    const requestDto = {
        RoomIds: occupancyConfig.propertyIds,
        StartDate: occupancyConfig.startDate,
        EndDate: occupancyConfig.endDate,
        GroupBy: occupancyConfig.groupBy
    };

    try {
        const response = await apiClient.post('/ReportForm/Occupancy/GetOccupancy', requestDto);
        return { points: response.data };
    } catch (error) {
        console.error('Error fetching occupancy data:', error);
        return { points: [] };
    }
  }

  if (type === 'occupancy_kpi') {
    const kpiConfig = config as OccupancyKpiConfig;
    const requestDto = {
        RoomIds: kpiConfig.propertyIds,
        Days: kpiConfig.lastDays
    };

    try {
        const response = await apiClient.post('/ReportForm/Occupancy/GetOccupancyKpi', requestDto);
        return response.data; // e.g., { occupancyRate: 85.5 }
    } catch (error) {
        console.error('Error fetching occupancy KPI data:', error);
        return { occupancyRate: 0 };
    }
  }

  if (type === 'revenue_kpi') {
    const kpiConfig = config as RevenueKpiConfig;
    const requestDto = {
        RoomIds: kpiConfig.propertyIds,
        Days: kpiConfig.lastDays
    };

    try {
        const response = await apiClient.post('/ReportForm/Revenue/GetRevenueKpi', requestDto);
        return response.data; // e.g., { totalRevenue: 12345 }
    } catch (error) {
        console.error('Error fetching revenue KPI data:', error);
        return { totalRevenue: 0 };
    }
  }

  if (type === 'revenue_source') {
    const sourceConfig = config as RevenueSourceConfig;
    const requestDto = {
        RoomIds: sourceConfig.propertyIds,
        Days: sourceConfig.lastDays
    };

    try {
        const response = await apiClient.post('/ReportForm/Revenue/GetRevenueSourceAnalysis', requestDto);
        return { points: response.data }; // e.g., [{ roomTitle: 'Room A', totalRevenue: 5000 }]
    } catch (error) {
        console.error('Error fetching revenue source data:', error);
        return { points: [] };
    }
  }

  if (type === 'occupancy_source') {
    const sourceConfig = config as OccupancySourceConfig;
    const requestDto = {
        RoomIds: sourceConfig.propertyIds,
        Days: sourceConfig.lastDays
    };

    try {
        const response = await apiClient.post('/ReportForm/Occupancy/GetOccupancySourceAnalysis', requestDto);
        return { points: response.data }; // e.g., [{ roomTitle: 'Room A', bookingCount: 5 }]
    } catch (error) {
        console.error('Error fetching occupancy source data:', error);
        return { points: [] };
    }
  }

  if (type === 'revenue_prediction') {
    const predConfig = config as RevenuePredictionConfig;
    const requestDto = {
        RoomIds: predConfig.propertyIds,
        ForecastDays: predConfig.forecastDays
    };

    try {
        const response = await apiClient.post('/ReportForm/Revenue/GetRevenuePrediction', requestDto);
        return response.data; // Expects { historicalPoints: [], predictedPoints: [] }
    } catch (error) {
        console.error('Error fetching revenue prediction data, returning mock data:', error);
        // Mock data on error
        const today = new Date();
        const historicalPoints = Array.from({ length: 90 }, (_, i) => {
            const date = new Date(today);
            date.setDate(today.getDate() - 90 + i);
            return { date: date.toISOString().split('T')[0], revenue: Math.random() * 1000 + 500 };
        });
        const lastHistoricalRevenue = historicalPoints[historicalPoints.length - 1]?.revenue ?? 500;
        const predictedPoints = Array.from({ length: predConfig.forecastDays }, (_, i) => {
            const date = new Date(today);
            date.setDate(today.getDate() + i + 1);
            return { date: date.toISOString().split('T')[0], revenue: lastHistoricalRevenue * (1 + (Math.random() - 0.4) * 0.1) };
        });
        return { historicalPoints, predictedPoints };
    }
  }

  if (type === 'occupancy_prediction') {
    const predConfig = config as OccupancyPredictionConfig;
    const requestDto = {
        RoomIds: predConfig.propertyIds,
        ForecastDays: predConfig.forecastDays
    };

    try {
        const response = await apiClient.post('/ReportForm/Occupancy/GetOccupancyPrediction', requestDto);
        return response.data; // Expects { historicalPoints: [], predictedPoints: [] }
    } catch (error) {
        console.error('Error fetching occupancy prediction data, returning mock data:', error);
        // Mock data on error
        const today = new Date();
        const historicalPoints = Array.from({ length: 90 }, (_, i) => {
            const date = new Date(today);
            date.setDate(today.getDate() - 90 + i);
            return { date: date.toISOString().split('T')[0], occupancyRate: Math.random() * 20 + 70 };
        });
        const lastHistoricalRate = historicalPoints[historicalPoints.length - 1]?.occupancyRate ?? 80;
        const predictedPoints = Array.from({ length: predConfig.forecastDays }, (_, i) => {
            const date = new Date(today);
            date.setDate(today.getDate() + i + 1);
            return { date: date.toISOString().split('T')[0], occupancyRate: Math.max(0, Math.min(100, lastHistoricalRate * (1 + (Math.random() - 0.5) * 0.1))) };
        });
        return { historicalPoints, predictedPoints };
    }
  }

  if (type === 'heatmap') {
    // Mock data for heatmap
    const points = Array.from({ length: 20 }, () => ({
        x: Math.random() * 100,
        y: Math.random() * 100,
        weight: Math.round(Math.random() * 20000 + 2000)
    }));
    return delay({ points });
  }

  // Fallback for unknown types
  return Promise.resolve({});
}


// Trivial id generator
function cryptoRandomId(): string {
  // Use browser crypto if available, else fallback
  try {
    const buf = new Uint8Array(8);
    // @ts-ignore
    (globalThis.crypto || (window as any).crypto).getRandomValues(buf);
    return Array.from(buf).map(b => b.toString(16).padStart(2, '0')).join('');
  } catch {
    return Math.random().toString(36).slice(2, 10);
  }
}
