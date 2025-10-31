<script setup lang="ts">
import { ref, onMounted } from 'vue';
import type { CategoryTreeDto, FaqArticleDto } from '@/modules/faq/types';
import { getCategoryTree } from '@/modules/faq/api';

const categories = ref<CategoryTreeDto[]>([]);
const currentArticle = ref<FaqArticleDto | null>(null);
const expanded = ref<Set<number>>(new Set());

function toggle(id:number){
  expanded.value.has(id) ? expanded.value.delete(id) : expanded.value.add(id);
}

onMounted(async () => {
  categories.value = await getCategoryTree(true); // 載入樹狀分類 + 文章
});
</script>

<template>
  <div class="container my-4">
    <div class="row">
      <div class="col-md-4">
        <ul class="list-unstyled">
          <li v-for="c in categories" :key="c.id" class="mb-2">
            <div class="d-flex align-items-center">
              <button class="btn btn-sm btn-outline-secondary me-2" @click="toggle(c.id)">
                {{ expanded.has(c.id) ? '－' : '＋' }}
              </button>
              <strong>{{ c.name }}</strong>
            </div>
            <ul v-if="expanded.has(c.id)" class="ms-4 list-unstyled">
              <li v-for="a in c.articles" :key="a.id" class="my-1">
                <a href="#" @click.prevent="currentArticle = a">{{ a.title }}</a>
              </li>
            </ul>
          </li>
        </ul>
      </div>
      <div class="col-md-8">
        <div v-if="currentArticle" class="card">
          <div class="card-body">
            <h5 class="card-title">{{ currentArticle.title }}</h5>
            <div class="card-text" v-html="currentArticle.content"></div>
          </div>
        </div>
        <div v-else class="text-muted">請從左側選擇一篇文章</div>
      </div>
    </div>
  </div>
</template>
