import { defineStore } from 'pinia';

export const useReportStore = defineStore('reportStore', {
    state: () => ({
        cards: [] as ReportCard[],
    }),
    actions: {
        addCard(card: ReportCard) {
            this.cards.push(card);
        },
        removeCard(id: string) {
            this.cards = this.cards.filter(c => c.id !== id);
        },
        updateCard(id: string, updates: Partial<ReportCard>) {
            const card = this.cards.find(c => c.id === id);
            if (card) Object.assign(card, updates);
        }
    }
});

// 通用的 ReportCard（用 kind 做鑑別 + 泛型承載 config/data）
export type ReportCard<K extends CardKind = CardKind> = {
    id: string;
    kind: K;
    meta: BaseCardMeta;
    config: CardTypeMap[K]['config'];
    data?: CardTypeMap[K]['data'];
};

// BaseCard 只放你已確定的 props：
export interface BaseCardMeta {
    title: string;
    subtitle?: string;
    loading?: boolean;
    actions?: Array<{ key: string; label: string }>;
    collapsible?: boolean;
}


// 以 kind 作為「類型鑑別子」
export type CardKind = 'revenue' | 'occupancy' | 'heatmap' /* 之後可加 */;

// 將「卡片類型」對應到它的 config/data 型別
export interface CardTypeMap {
    revenue: { config: RevenueConfig; data: RevenuePoint[] };
    occupancy: { config: OccupancyConfig; data: OccupancySlice[] };
    heatmap: { config: HeatmapConfig; data: HeatmapPoint[] };
}

export interface RevenueConfig {
    propertyIds: string[];// 多房源
    range: { start: string; end: string };
    groupBy: 'day' | 'week' | 'month';
    chartType: 'line' | 'bar' | 'pie';
}
export interface RevenuePoint { Date: string; Revenue: number; }

export interface OccupancyConfig {
    propertyIds: string[];// 多房源
    range: { start: string; end: string };
    breakdownBy?: 'roomType' | 'channel';
}
export interface OccupancySlice { label: string; value: number; }

export interface HeatmapConfig {
    propertyIds: string[];// 多房源
    center: { lat: number; lng: number };
    zoom: number;
}
export interface HeatmapPoint { lat: number; lng: number; weight: number };