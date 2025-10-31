export type FaqArticleDto = {
  id: number;
  title: string;
  content: string;
  categoryId: number;
  summary: string;
  isActive: boolean;
};

export type CategoryTreeDto = {
  id: number;
  name: string;
  isActive: boolean;
  children: CategoryTreeDto[];
  articles: FaqArticleDto[];
};

export type Paged<T> = {
  page: number;
  pageSize: number;
  total: number;
  items: T[];
};
