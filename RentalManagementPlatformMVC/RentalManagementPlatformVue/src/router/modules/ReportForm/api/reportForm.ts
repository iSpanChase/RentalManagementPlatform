
import axios from 'axios';

// src/modules/ReportForm/api/reportForm.ts
export type CardType = 'revenue' | 'occupancy' | 'heatmap';

export interface BaseDraft {
  type: CardType;
  title: string;
  subtitle?: string;
}

export interface RevenueConfig {
  propertyIds: number[];
  range: { start: string; end: string };
  groupBy: 'day' | 'week' | 'month';
  chartType: 'line' | 'bar' | 'pie';
}

export interface OccupancyConfig {
  propertyIds: string[];
  range: { start: string; end: string };
  breakdownBy?: 'roomType' | 'channel';
}

export interface HeatmapConfig {
  propertyIds: string[];
  center: { lat: number; lng: number };
  zoom: number;
}

export type CardConfig = RevenueConfig | OccupancyConfig | HeatmapConfig;

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
        StartDate: revenueConfig.range.start,
        EndDate: revenueConfig.range.end,
        GroupBy: revenueConfig.groupBy
    };
    
    try {
        const response = await axios.post('/api/ReportForm/Revenue/GetRevenue', requestDto);
        // The backend returns an array of { Date, Revenue }, wrap it in a 'points' property
        return { points: response.data };
    } catch (error) {
        console.error('Error fetching revenue data:', error);
        // Return empty points on error
        return { points: [] };
    }
  }
  
  if (type === 'occupancy') {
    // Mock data for occupancy
    const slices = [
      { label: '單人房', value: Math.round(Math.random() * 40 + 20) },
      { label: '雙人房', value: Math.round(Math.random() * 40 + 20) },
      { label: '家庭房', value: Math.round(Math.random() * 20 + 10) },
    ];
    return delay({ slices });
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
