import axios from 'axios';
import type { CategoryTreeDto, FaqArticleDto, Paged } from './types';

// 建立 axios 實例
const api = axios.create({
    baseURL: import.meta.env.VITE_API_BASE || 'https://localhost:5001',
    withCredentials: false,
    timeout: 10000
});

// 取得 FAQ 樹狀分類
export async function getCategoryTree(includeArticles = true) {
    const { data } = await api.get<CategoryTreeDto[]>('/api/faqcategories/tree', {
        params: { includeArticles }
    });
    return data;
}

// 依分類取得文章
export async function getArticlesByCategory(categoryId: number) {
    const { data } = await api.get<FaqArticleDto[]>(`/api/faqcategories/${categoryId}/articles`);
    return data;
}

// 查詢文章（關鍵字/分頁）
export async function queryArticles(params: { search?: string; categoryId?: number; page?: number; pageSize?: number }) {
    const { data } = await api.get<Paged<FaqArticleDto>>('/api/faqarticles', { params });
    return data;
}

// 取得單篇文章
export async function getArticle(id: number) {
    const { data } = await api.get<FaqArticleDto>(`/api/faqarticles/${id}`);
    return data;
}
