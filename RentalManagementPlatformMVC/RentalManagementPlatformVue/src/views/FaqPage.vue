<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import axios from 'axios';

type FaqArticleDto = {
  id: number;
  title: string;
  summary?: string | null;
  content: string;
  categoryId: number;
  isActive?: boolean;
};

type CategoryTreeDto = {
  id: number;
  name: string;
  isActive: boolean | null;
  children: CategoryTreeDto[];
  articles: FaqArticleDto[];
};

const loading = ref(true);
const error = ref<string | null>(null);
const q = ref('');                                // 搜尋關鍵字
const categories = ref<CategoryTreeDto[]>([]);    // 從 API 拿到的樹

// Axios 實例
const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE || 'https://localhost:5001',
  timeout: 15000,
});

// 讀取分類樹（含文章）
async function loadTree() {
  loading.value = true;
  error.value = null;
  try {
    const { data } = await api.get<CategoryTreeDto[]>('/api/faqcategories/tree', {
      params: { includeArticles: true }
    });
    categories.value = data;
  } catch (e: any) {
    error.value = e?.message ?? '載入 FAQ 失敗';
  } finally {
    loading.value = false;
  }
}

// 只保留父主題（ParentId = null 的節點：API 已組好 roots）
const parentSections = computed(() => categories.value);

// 搜尋：在所有層級（子主題→問題）做簡單過濾
const filteredSections = computed(() => {
  const keyword = q.value.trim().toLowerCase();
  if (!keyword) return parentSections.value;

  // 深拷貝 + 過濾
  const clone = (c: CategoryTreeDto): CategoryTreeDto => ({
    id: c.id,
    name: c.name,
    isActive: c.isActive ?? true,
    children: c.children.map(clone),
    articles: c.articles
  });

  const match = (text?: string | null) =>
    (text ?? '').toLowerCase().includes(keyword);

  const filterNode = (node: CategoryTreeDto): CategoryTreeDto | null => {
    const copy = clone(node);

    // 過濾子主題
    copy.children = copy.children
      .map(filterNode)
      .filter((x): x is CategoryTreeDto => !!x);

    // 過濾問題（標題/摘要/內容命中其一）
    copy.articles = copy.articles.filter(a =>
      match(a.title) || match(a.summary) || match(a.content)
    );

    // 若自己名稱命中，或仍有子節點/文章，就保留
    const selfHit = match(copy.name);
    if (selfHit || copy.children.length || copy.articles.length) return copy;
    return null;
  };

  return parentSections.value
    .map(filterNode)
    .filter((x): x is CategoryTreeDto => !!x);
});

// 產生穩定的 DOM id（用於 Accordion 的 target）
function uid(parts: (string | number)[]) {
  return parts.join('-').replace(/\s+/g, '-').toLowerCase();
}

// 頁面載入
onMounted(loadTree);

// 捲動到父主題
function scrollToSection(catId: number) {
  const el = document.getElementById('section-' + catId);
  if (el) el.scrollIntoView({ behavior: 'smooth', block: 'start' });
}
</script>

<template>
  <div class="faq-page container py-5">
    <!-- Title -->
    <h1 class="display-4 text-center fw-bold mb-2">FAQ</h1>
    <p class="text-center text-muted mb-5">常見問題依主題分類，點擊問題即可展開內容</p>

    <!-- Top tools -->
    <div class="row g-3 align-items-center mb-4">
      <div class="col-12 col-lg-8">
        <div class="input-group">
          <span class="input-group-text">搜尋</span>
          <input
            v-model="q"
            type="text"
            class="form-control"
            placeholder="輸入關鍵字（主題 / 標題 / 摘要 / 內容）"
          />
          <button class="btn btn-outline-secondary" @click="q = ''" :disabled="!q">清除</button>
        </div>
      </div>

      <!-- Anchor TOC -->
      <div class="col-12 col-lg-4">
        <div class="d-flex flex-wrap gap-2 justify-content-lg-end">
          <button
            v-for="sec in parentSections"
            :key="sec.id"
            class="btn btn-sm btn-outline-primary"
            @click="scrollToSection(sec.id)"
          >
            {{ sec.name }}
          </button>
        </div>
      </div>
    </div>

    <!-- Loading / Error -->
    <div v-if="loading" class="text-center text-muted py-5">載入中…</div>
    <div v-else-if="error" class="alert alert-danger">{{ error }}</div>

    <!-- Content -->
    <template v-else>
      <section
        v-for="parent in filteredSections"
        :key="parent.id"
        :id="'section-' + parent.id"
        class="mb-5"
      >
        <!-- Parent header -->
        <div class="d-flex align-items-center mb-2">
          <div class="flex-grow-1">
            <h2 class="h3 fw-bold mb-1">{{ parent.name }}</h2>
            <div class="separator"></div>
          </div>
        </div>

        <!-- Optional description（若你未來想加，可在這裡放 parent.description） -->
        <!-- <p class="text-muted">此主題包含館內開放資訊…</p> -->

        <!-- 子主題區塊：每個子主題都是一組 Accordion -->
        <div
          v-for="child in parent.children"
          :key="child.id"
          class="mb-4"
        >
          <h3 class="h5 fw-semibold mb-2">{{ child.name }}</h3>

          <div class="accordion" :id="uid(['acc', parent.id, child.id])">
            <!-- 問題（文章） -->
            <div
              v-for="art in child.articles"
              :key="art.id"
              class="accordion-item"
            >
              <h2 class="accordion-header" :id="uid(['hd', child.id, art.id])">
                <button
                  class="accordion-button collapsed"
                  type="button"
                  data-bs-toggle="collapse"
                  :data-bs-target="'#' + uid(['col', child.id, art.id])"
                  aria-expanded="false"
                  :aria-controls="uid(['col', child.id, art.id])"
                >
                  <span class="fw-semibold">{{ art.title }}</span>
                </button>
              </h2>
              <div
                :id="uid(['col', child.id, art.id])"
                class="accordion-collapse collapse"
                :aria-labelledby="uid(['hd', child.id, art.id])"
                :data-bs-parent="'#' + uid(['acc', parent.id, child.id])"
              >
                <div class="accordion-body">
                  <p v-if="art.summary" class="text-muted mb-2">{{ art.summary }}</p>
                  <div v-html="art.content"></div>
                </div>
              </div>
            </div>

            <!-- 沒有問題的子主題 -->
            <div v-if="!child.articles.length" class="text-muted small px-2 py-3 border rounded">
              此子主題目前沒有問題。
            </div>
          </div>
        </div>

        <!-- 父主題底下若直接有文章（不少資料會這樣），也顯示一組 Accordion -->
        <div v-if="parent.articles?.length">
          <h3 class="h5 fw-semibold mb-2">更多問題</h3>
          <div class="accordion" :id="uid(['acc', parent.id, 'root'])">
            <div
              v-for="art in parent.articles"
              :key="art.id"
              class="accordion-item"
            >
              <h2 class="accordion-header" :id="uid(['hd', parent.id, 'root', art.id])">
                <button
                  class="accordion-button collapsed"
                  type="button"
                  data-bs-toggle="collapse"
                  :data-bs-target="'#' + uid(['col', parent.id, 'root', art.id])"
                  aria-expanded="false"
                  :aria-controls="uid(['col', parent.id, 'root', art.id])"
                >
                  <span class="fw-semibold">{{ art.title }}</span>
                </button>
              </h2>
              <div
                :id="uid(['col', parent.id, 'root', art.id])"
                class="accordion-collapse collapse"
                :aria-labelledby="uid(['hd', parent.id, 'root', art.id])"
                :data-bs-parent="'#' + uid(['acc', parent.id, 'root'])"
              >
                <div class="accordion-body">
                  <p v-if="art.summary" class="text-muted mb-2">{{ art.summary }}</p>
                  <div v-html="art.content"></div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      <!-- 沒資料 -->
      <div v-if="!filteredSections.length" class="text-center text-muted py-5">
        查無符合條件的結果。
      </div>
    </template>
  </div>
</template>

<style scoped>
.faq-page .separator {
  height: 4px;
  background: linear-gradient(90deg, #7a5cff, #b794f6 60%, transparent);
  border-radius: 2px;
  margin-bottom: .75rem;
}
.accordion-button {
  padding-top: .8rem;
  padding-bottom: .8rem;
}
.accordion-item + .accordion-item {
  border-top: 0;
}
</style>
