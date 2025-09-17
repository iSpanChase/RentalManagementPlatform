(function () {
    // ---------- helpers ----------
    const metaToken = () =>
        document.querySelector('meta[name="request-verification-token"]')?.content || "";

    // 兼容大小寫/異名欄位的小工具
    const pick = (obj, ...keys) => {
        for (const k of keys) if (obj && k in obj) return obj[k];
        return undefined;
    };

    document.addEventListener('DOMContentLoaded', async () => {
        try {
            await loadParents();
            await loadChildren();
            await loadArticles();   // 先把資料載入
            await loadFeedback();
        } catch (err) {
            console.error(err);
            alert('初始化資料失敗：' + err.message);
        }

        // 圖表初始化放這裡，且一定包 try/catch
        try {
            initCharts();
        } catch (e) {
            console.warn('Chart init skipped:', e);
        }
    });

    // 統一的 fetch（偵測非 JSON 回應，丟出可讀錯誤）
    async function jsonFetch(method, url, body) {
        const headers = { 'Content-Type': 'application/json' };
        const token = metaToken();
        if (token) headers['RequestVerificationToken'] = token;

        const res = await fetch(url, {
            method,
            headers,
            body: body ? JSON.stringify(body) : null
        });

        const ct = res.headers.get('content-type') || '';
        if (!res.ok) {
            const text = await res.text();
            throw new Error(`HTTP ${res.status}. Expected JSON, got: ${ct}. Body head: ${text.slice(0, 200)}`);
        }
        if (!ct.includes('application/json')) {
            const text = await res.text();
            throw new Error(`Expected JSON but got ${ct}. Body head: ${text.slice(0, 200)}`);
        }
        return res.json();
    }

    // 舊介面轉接到新介面（減少你其他地方改動）
    const json = (method, url, body) => jsonFetch(method, url, body);
    const del = (url) => jsonFetch('DELETE', url);

    // ===== 父分類 =====
    async function loadParents() {
        const data = await jsonFetch('GET', '/FAQ/Admin/Categories/Parents');
        const list = data.data || data.Data || [];
        const tbody = document.getElementById('parentList');
        if (!tbody) return;

        tbody.innerHTML = '';
        list.forEach(p => {
            const id = pick(p, 'FaqCategoriesId', 'faqCategoriesId', 'CategoryId', 'categoryId', 'Id', 'id');
            const name = pick(p, 'Name', 'name');
            const tr = document.createElement('tr');
            tr.innerHTML = `
        <td>${id ?? ''}</td>
        <td>${name ?? ''}</td>
        <td>
          <button class="btn btn-sm btn-outline-primary me-1" data-act="edit" data-id="${id ?? ''}" data-name="${name ?? ''}">編輯</button>
          <button class="btn btn-sm btn-outline-danger" data-act="del" data-id="${id ?? ''}">刪除</button>
        </td>`;
            tbody.appendChild(tr);
        });

        await fillParentsDropdown();
    }

    async function fillParentsDropdown() {
        const data = await jsonFetch('GET', '/FAQ/Admin/Articles/AllParentsForDropdown');
        const list = data.data || data.Data || [];
        const sel = document.getElementById('parentDropdown');
        if (!sel) return;

        sel.innerHTML = `<option value="">— 選擇父分類 —</option>`;
        list.forEach(p => {
            const id = pick(p, 'CategoryId', 'categoryId', 'FaqCategoriesId', 'faqCategoriesId', 'Id', 'id');
            const name = pick(p, 'Name', 'name', 'Text', 'text');
            const opt = document.createElement('option');
            opt.value = id ?? '';
            opt.textContent = name ?? '';
            sel.appendChild(opt);
        });
    }

    document.addEventListener('click', async (e) => {
        const t = e.target;
        if (t.matches('#btnSaveParent')) {
            try {
                const name = document.getElementById('parentName').value.trim();
                const id = document.getElementById('parentId').value;
                const dto = { categoryId: id ? Number(id) : null, name, parentId: null };
                const res = await json('POST', '/FAQ/Admin/Categories/Upsert', dto);
                alert((res.message || res.Message) ?? '已處理');
                resetParentForm();
                await loadParents();
                await loadChildren(); // 連動刷新
            } catch (err) {
                console.error(err);
                alert('父分類新增/更新失敗：' + err.message);
            }
        }
        if (t.matches('#btnResetParent')) {
            resetParentForm();
        }
        if (t.dataset.act === 'edit' && t.closest('#parentList')) {
            document.getElementById('parentId').value = t.dataset.id;
            document.getElementById('parentName').value = t.dataset.name;
        }
        if (t.dataset.act === 'del' && t.closest('#parentList')) {
            if (confirm('確認刪除此父分類？')) {
                try {
                    const id = t.dataset.id;
                    const res = await del(`/FAQ/Admin/Categories/Delete?id=${id}`);
                    alert((res.message || res.Message) ?? '已刪除');
                    await loadParents();
                    await loadChildren();
                } catch (err) {
                    console.error(err);
                    alert('刪除失敗：' + err.message);
                }
            }
        }
    });

    function resetParentForm() {
        document.getElementById('parentId').value = '';
        document.getElementById('parentName').value = '';
    }

    // ===== 子分類 =====
    async function loadChildren() {
        const data = await jsonFetch('GET', '/FAQ/Admin/Categories/Children');
        const list = data.data || data.Data || [];
        const tbody = document.getElementById('childList');
        if (!tbody) return;

        tbody.innerHTML = '';
        list.forEach(c => {
            const id = pick(c, 'CategoryId', 'categoryId', 'FaqCategoriesId', 'faqCategoriesId', 'Id', 'id');
            const parentId = pick(c, 'ParentId', 'parentId');
            const parentName = pick(c, 'ParentName', 'parentName');
            const name = pick(c, 'Name', 'name');
            const tr = document.createElement('tr');
            tr.innerHTML = `
        <td>${id ?? ''}</td>
        <td>${parentName ?? ''}</td>
        <td>${name ?? ''}</td>
        <td>
          <button class="btn btn-sm btn-outline-primary me-1" data-act="edit-child"
            data-id="${id ?? ''}" data-name="${name ?? ''}" data-parent="${parentId ?? ''}">編輯</button>
          <button class="btn btn-sm btn-outline-danger" data-act="del-child" data-id="${id ?? ''}">刪除</button>
        </td>`;
            tbody.appendChild(tr);
        });

        await fillChildrenDropdown();
    }

    async function fillChildrenDropdown() {
        const data = await jsonFetch('GET', '/FAQ/Admin/Articles/AllChildrenForDropdown');
        const list = data.data || data.Data || [];
        const sel = document.getElementById('childDropdown');
        if (!sel) return;

        sel.innerHTML = `<option value="">— 選擇子分類 —</option>`;
        list.forEach(c => {
            const id = pick(c, 'CategoryId', 'categoryId', 'FaqCategoriesId', 'faqCategoriesId', 'Id', 'id');
            const text = pick(c, 'Text', 'text', 'Name', 'name');
            const opt = document.createElement('option');
            opt.value = id ?? '';
            opt.textContent = text ?? '';
            sel.appendChild(opt);
        });
    }

    document.addEventListener('click', async (e) => {
        const t = e.target;
        if (t.matches('#btnSaveChild')) {
            try {
                const id = document.getElementById('childId').value;
                const name = document.getElementById('childName').value.trim();
                const parentId = document.getElementById('parentDropdown').value || null;
                if (!parentId) { alert('請選擇父分類'); return; }
                const dto = { categoryId: id ? Number(id) : null, name, parentId: parentId ? Number(parentId) : null };
                const res = await json('POST', '/FAQ/Admin/Categories/Upsert', dto);
                alert((res.message || res.Message) ?? '已處理');
                resetChildForm();
                await loadChildren();
            } catch (err) {
                console.error(err);
                alert('子分類新增/更新失敗：' + err.message);
            }
        }
        if (t.dataset.act === 'edit-child') {
            document.getElementById('childId').value = t.dataset.id || '';
            document.getElementById('childName').value = t.dataset.name || '';
            document.getElementById('parentDropdown').value = t.dataset.parent || '';
        }
        if (t.dataset.act === 'del-child') {
            if (confirm('確認刪除此子分類？（若有文章將無法刪除）')) {
                try {
                    const id = t.dataset.id;
                    const res = await del(`/FAQ/Admin/Categories/Delete?id=${id}`);
                    alert((res.message || res.Message) ?? '已刪除');
                    await loadChildren();
                } catch (err) {
                    console.error(err);
                    alert('刪除失敗：' + err.message);
                }
            }
        }
    });

    function resetChildForm() {
        document.getElementById('childId').value = '';
        document.getElementById('childName').value = '';
        document.getElementById('parentDropdown').value = '';
    }


    // ===== 文章 =====
    async function loadArticles() {
        const data = await jsonFetch('GET', '/FAQ/Admin/Articles/List');
        const list = data.data || data.Data || [];
        const tbody = document.getElementById('articleList');
        if (!tbody) return;

        tbody.innerHTML = '';
        list.forEach(a => {
            const id = pick(a, 'ArticleId', 'articleId', 'Id', 'id');
            const categoryName = pick(a, 'CategoryName', 'categoryName');
            const title = pick(a, 'Title', 'title');
            const status = pick(a, 'Status', 'status', 'IsActive', 'isActive'); // 兼容老資料
            let statusText = '';
            if (typeof status === 'boolean') {
                // 之前你用 bool 的判斷
                statusText = status ? 'published' : 'draft';
            } else if (status) {
                // 這裡轉換成中文
                switch (status.toLowerCase()) {
                    case 'published':
                        statusText = '發布';
                        break;
                    case 'draft':
                        statusText = '草稿';
                        break;
                    case 'archived':
                        statusText = '封存';
                        break;
                    default:
                        statusText = status; // 其他值就原樣輸出
                }
            }

            const tr = document.createElement('tr');
            tr.innerHTML = `
        <td>${id ?? ''}</td>
        <td>${categoryName ?? ''}</td>
        <td>${title ?? ''}</td>
        <td>${statusText}</td>
        <td>
          <button class="btn btn-sm btn-outline-primary me-1" data-act="edit-article" data-id="${id ?? ''}">編輯</button>
          <button class="btn btn-sm btn-outline-danger" data-act="del-article" data-id="${id ?? ''}">刪除</button>
        </td>`;
            tbody.appendChild(tr);
        });

        await fillFeedbackArticleDropdown();
    }

    document.addEventListener('click', async (e) => {
        const t = e.target;
        if (t.matches('#btnSaveArticle')) {
            try {
                const id = document.getElementById('articleId').value;
                const categoryId = document.getElementById('childDropdown').value;
                const title = document.getElementById('articleTitle').value.trim();
                const content = document.getElementById('articleContent').value;
                const status = document.getElementById('articleStatus')?.value || 'draft'; // 需要在 View 放下拉

                const dto = {
                    articleId: id ? Number(id) : null,
                    categoryId: categoryId ? Number(categoryId) : 0,
                    title,
                    content,
                    status
                };
                const res = await json('POST', '/FAQ/Admin/Articles/Upsert', dto);
                alert((res.message || res.Message) ?? '已處理');
                resetArticleForm();
                await loadArticles();
            } catch (err) {
                console.error(err);
                alert('文章新增/更新失敗：' + err.message);
            }
        }

        if (t.dataset.act === 'edit-article') {
            (async () => {
                try {
                    const id = t.dataset.id;
                    const r = await jsonFetch('GET', `/FAQ/Admin/Articles/Get?id=${id}`);
                    if (!(r.success || r.Success)) { alert(r.message || r.Message || '讀取文章失敗'); return; }
                    const a = r.data || r.Data || {};
                    document.getElementById('articleId').value = pick(a, 'ArticleId', 'articleId', 'Id', 'id') ?? '';
                    document.getElementById('childDropdown').value = pick(a, 'CategoryId', 'categoryId') ?? '';
                    document.getElementById('articleTitle').value = pick(a, 'Title', 'title') ?? '';
                    document.getElementById('articleContent').value = pick(a, 'Content', 'content') ?? '';
                    const s = pick(a, 'Status', 'status');
                    const sel = document.getElementById('articleStatus');
                    if (sel) sel.value = s || 'draft';
                } catch (err) {
                    console.error(err);
                    alert('載入文章失敗：' + err.message);
                }
            })();
        }

        if (t.dataset.act === 'del-article') {
            if (confirm('確認刪除此文章？（相關回饋將一併刪除）')) {
                try {
                    const id = t.dataset.id;
                    const res = await del(`/FAQ/Admin/Articles/Delete?id=${id}`);
                    alert((res.message || res.Message) ?? '已刪除');
                    await loadArticles();
                    await loadFeedback(); // 連動
                } catch (err) {
                    console.error(err);
                    alert('刪除失敗：' + err.message);
                }
            }
        }
    });

    function resetArticleForm() {
        document.getElementById('articleId').value = '';
        document.getElementById('childDropdown').value = '';
        document.getElementById('articleTitle').value = '';
        document.getElementById('articleContent').value = '';
        const sel = document.getElementById('articleStatus');
        if (sel) sel.value = 'draft';
    }

    // ===== 回饋 =====
    async function fillFeedbackArticleDropdown() {
        const res = await fetch('/FAQ/Admin/Feedback/AllArticlesForDropdown');
        const data = await res.json();
        const list = data.data || data.Data;
        const sel = document.getElementById('feedbackArticleFilter');
        if (!sel) return;
        sel.innerHTML = `<option value="">— 全部文章 —</option>`;
        list.forEach(a => {
            const opt = document.createElement('option');
            opt.value = a.articleId;
            opt.textContent = a.title;
            sel.appendChild(opt);
        });
    }

    function renderSentiment(val) {
        if (!val) return '';
        const sentiment = val.toLowerCase();
        switch (sentiment) {
            case 'positive':
                return `<i class="fa-regular fa-face-smile text-success"></i> `;
            case 'neutral':
                return `<i class="fa-regular fa-face-meh text-secondary"></i> `;
            case 'negative':
                return `<i class="fa-regular fa-face-frown text-danger"></i> `;
            default:
                return `<i class="fa-regular fa-circle-question text-info"></i> ${val}`;
        }
    }

    async function loadFeedback() {
        const sel = document.getElementById('feedbackArticleFilter');
        const aid = sel && sel.value ? `?articleId=${sel.value}` : '';
        const res = await fetch('/FAQ/Admin/Feedback/List' + aid);
        const data = await res.json();
        const list = data.data || data.Data;
        const tbody = document.getElementById('feedbackList');
        if (!tbody) return;
        tbody.innerHTML = '';
        list.forEach(f => {
            const tr = document.createElement('tr');
            tr.innerHTML = `
        <td>${f.faqFeedbackId}</td>
        <td>[${f.articleId}] ${f.articleTitle}</td>
        <td>${f.userId ?? ''}</td>
        <td>${renderSentiment(f.sentiment)}</td>
        <td>${f.reason ?? ''}</td>
        <td>${f.contactEmail ?? ''}</td>
        <td>${(f.createdAt || '').toString().replace('T', ' ').substring(0, 19)}</td>
        <td><button class="btn btn-sm btn-outline-danger" data-act="del-feedback" data-id="${f.faqFeedbackId}">刪除</button></td>`;
            tbody.appendChild(tr);
        });
    }

    document.addEventListener('click', async (e) => {
        const t = e.target;
        if (t.matches('#btnReloadFeedback')) {
            loadFeedback();
        }
        if (t.dataset.act === 'del-feedback') {
            if (confirm('確認刪除此回饋？')) {
                const id = t.dataset.id;
                const res = await del(`/FAQ/Admin/Feedback/Delete?id=${id}`);
                alert(res.message || res.Message);
                loadFeedback();
            }
        }
    });

    // ===== 頁面初始化 =====
    document.addEventListener('DOMContentLoaded', async () => {
        await loadParents();
        await loadChildren();
        await loadArticles();
        await fillFeedbackArticleDropdown();
        await loadFeedback();

        const filterSel = document.getElementById('feedbackArticleFilter');
        if (filterSel) {
            filterSel.addEventListener('change', loadFeedback);
        }
    });
})();
