(function () {
    const CardBus = new Map(); // key: cardId -> {el, chart, state}
    let editingCardId = null;  // 編輯中的卡 id（null 表示新增模式）

    // === 工具：收集當前「新增/編輯」面板的狀態，組合成送 API 與重建卡片所需的結構 ===
    function collectPanelState() {
        const timeFd = ReportTime.collect("#reportControls");
        const filterFd = ReportConfig.collectFilters({ filterPanel: '#reportFilterPanel' });

        const title = document.querySelector('#cardTitleInput').value?.trim() || '';
        const typeSel = document.querySelector('#globalReportType');
        const chartType = typeSel ? typeSel.value : 'bar';

        // ⭐ 報表種類：優先從 collectFilters 拿，拿不到就從下拉保底
        const reportId =
            filterFd.get('ReportId') ||
            document.querySelector('[name="ReportId"]')?.value ||
            document.querySelector('#reportSelect')?.value ||
            document.querySelector('#reportId')?.value ||
            '';

        const def = ReportConfig.get(reportId); // 依 ReportId 取 base/endpoint/title

        // 合併 FormData
        const fd = new FormData();
        for (const [k, v] of timeFd.entries()) fd.append(k, v);
        for (const [k, v] of filterFd.entries()) fd.append(k, v);
        // ⭐ 確保 ReportId 一定在表單裡（以便後續序列化/我的最愛）
        if (!fd.get('ReportId') && reportId) fd.append('ReportId', reportId);

        return {
            title: title || def?.title || '未命名卡片',
            chartType,
            reportId,
            base: def?.base,
            endpoint: def?.endpoint,
            formData: fd
        };
    }

    // === UI：面板顯示 / 隱藏 ===
    function showPanel({ forEdit = false, cardState = null } = {}) {
        const panel = document.querySelector('#reportControls');
        panel.hidden = false;
        document.querySelector('#btnConfirmAdd').hidden = forEdit;
        document.querySelector('#btnConfirmEdit').hidden = !forEdit;

        if (forEdit && cardState) {
            // 把「卡片名稱」帶回面板
            document.querySelector('#cardTitleInput').value = cardState.title || '';
            // 你的時間/篩選原本沒有「回填」API，這裡先略。若要完整回填，可在 ReportTime/ReportConfig 另外加 set()。
        }
    }
    function hidePanel() {
        const panel = document.querySelector('#reportControls');
        panel.hidden = true;
        editingCardId = null;
        // 清空只清「名稱」即可；其他選項維持使用者原先挑選流程
        document.querySelector('#cardTitleInput').value = '';
    }

    // === 建卡 ===
    async function addCard() {
        const st = collectPanelState();
        await createOrUpdateCard({ state: st });
        hidePanel();
    }

    // === 編輯卡（更新既有卡片內容，不新增） ===
    async function confirmEdit() {
        if (!editingCardId) return;
        const st = collectPanelState();
        await createOrUpdateCard({ state: st, cardId: editingCardId });
        hidePanel();
    }

    // === 共用：建立或更新一張卡 ===
    async function createOrUpdateCard({ state, cardId = null }) {
        if (!state?.base || !state?.endpoint) {
            alert('找不到報表設定（base/endpoint）。請確認報表定義是否已註冊。');
            return;
        }
        if (cardId) {
            // 更新：直接重繪
            const card = CardBus.get(cardId);
            if (!card) return;
            await card.update(state); // 下面會在 ReportCard 擴增 update()
        } else {
            // 新增
            const instance = await ReportCard.createCard({
                base: state.base,
                endpoint: state.endpoint,
                title: state.title,
                type: state.chartType,
                formData: state.formData
            }); // 你原有 createCard 已存在，下一段我會升級讓它回傳 instance 與提供編輯/刪除控制。 :contentReference[oaicite:10]{index = 10}

            // 註冊與快取
            CardBus.set(instance.id, instance);
        }
    }

    // === 事件繫結（工具列） ===
    document.querySelector('#btnAddCard').addEventListener('click', () => {
        showPanel({ forEdit: false });
    });
    document.querySelector('#btnCancelPanel').addEventListener('click', hidePanel);
    document.querySelector('#btnConfirmAdd').addEventListener('click', addCard);
    document.querySelector('#btnConfirmEdit').addEventListener('click', confirmEdit);

    // === 我 的 最 愛：清單載入 ===
    async function refreshFavorites() {
        // 你已經有 _fetchPostJSON，可沿用；也可改 GET，看你後端怎麼設計。 :contentReference[oaicite:11]{index = 11}
        const list = await ReportConfig._fetchPostJSON('/ReportForm/ReportForm/FavoritesList', null);
        const sel = document.querySelector('#favSelect');
        sel.innerHTML = '<option value="">（選擇我的最愛）</option>';
        list.forEach(x => {
            const opt = document.createElement('option');
            opt.value = x.id;
            opt.textContent = x.name;
            sel.appendChild(opt);
        });
    }

    // === 我 的 最 愛：儲存（把目前頁面所有卡片序列化後送出） ===
    async function saveFavorite() {
        const name = prompt('請為「我的最愛」命名：');
        if (!name) return;
        const snapshot = [];
        CardBus.forEach(({ state }) => snapshot.push(state));
        const ok = confirm(`將以「${name}」儲存目前頁面 ${snapshot.length} 張卡片，確認？`);
        if (!ok) return;

        await ReportConfig._fetchPostJSON('/ReportForm/ReportForm/FavoritesSave', {
            name,
            cards: snapshot
        }); // 你可在後端定義接收 DTO，將 cards[] 存 JSON。 :contentReference[oaicite:12]{index = 12}

        await refreshFavorites();
        alert('已儲存到我的最愛。');
    }

    // === 我 的 最 愛：載入（清空現有卡；用快照重建） ===
    async function loadFavorite() {
        const favId = document.querySelector('#favSelect').value;
        if (!favId) return;
        if (!confirm('載入將清空目前所有卡片並以選定的我的最愛取代，確定？')) return;

        const fav = await ReportConfig._fetchPostJSON('/ReportForm/ReportForm/FavoritesGet', { id: favId });
        // 清空
        document.querySelector('#reportCardsRow').innerHTML = '';
        CardBus.clear();

        for (const st of fav.cards) {
            // FormData 重建
            const fd = new FormData();
            Object.entries(st.form).forEach(([k, v]) => fd.append(k, v));
            await createOrUpdateCard({
                state: {
                    title: st.title,
                    chartType: st.chartType,
                    reportId: st.reportId,
                    base: st.base,
                    endpoint: st.endpoint,
                    formData: fd
                }
            });
        }
        alert('載入完成。');
    }

    // === 我 的 最 愛：刪除 ===
    async function deleteFavorite() {
        const favId = document.querySelector('#favSelect').value;
        if (!favId) return;
        if (!confirm('確定要刪除這個我的最愛嗎？此動作無法復原。')) return;
        await ReportConfig._fetchPostJSON('/ReportForm/ReportForm/FavoritesDelete', { id: favId });
        await refreshFavorites();
        alert('已刪除。');
    }

    document.querySelector('#btnFavSave').addEventListener('click', saveFavorite);
    document.querySelector('#btnFavLoad').addEventListener('click', loadFavorite);
    document.querySelector('#btnFavDelete').addEventListener('click', deleteFavorite);

    // 初始化：載入我的最愛清單
    refreshFavorites();

    // === 提供給 ReportCard（下面擴充）使用：進入「編輯模式」 ===
    window.__enterEditCard = function (cardId) {
        const inst = CardBus.get(cardId);
        if (!inst) return;
        editingCardId = cardId;
        showPanel({ forEdit: true, cardState: inst.state });

        // ⭐ 把目前卡片的報表種類選回面板並觸發 UI 重建
        const reportSel =
            document.querySelector('#globalReportId') ||
            document.querySelector('#reportSelect') ||
            document.querySelector('#reportId') ||
            document.querySelector('[name="ReportId"]');
        if (reportSel) {
            reportSel.value = inst.state.reportId || '';
            reportSel.dispatchEvent(new Event('change')); // 讓 ReportConfig 依新報表重建篩選區
        }
    };

    // === 提供給 ReportCard 使用：刪除卡 ===
    window.__deleteCard = function (cardId) {
        const inst = CardBus.get(cardId);
        if (!inst) return;
        const ok = confirm(`確定要刪除「${inst.state.title}」嗎？`);
        if (!ok) return;
        inst.el.remove();
        CardBus.delete(cardId);
    };
})();
