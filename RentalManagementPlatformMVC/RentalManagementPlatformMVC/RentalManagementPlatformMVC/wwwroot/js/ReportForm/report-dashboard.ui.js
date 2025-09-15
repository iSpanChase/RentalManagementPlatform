(function () {
    const CardBus = new Map(); // key: cardId -> {el, chart, state}
    let editingCardId = null;  // 編輯中的卡 id（null 表示新增模式）

    // === 工具：收集當前「新增/編輯」面板的狀態，組合成送 API 與重建卡片所需的結構 ===
    function collectPanelState() {
        // 先收集篩選，等一下要從裡面讀 ReportId
        const filterFd = ReportConfig.collectFilters({ filterPanel: '#reportFilterPanel' });

        // ⭐ 報表種類：優先從 collectFilters 拿，拿不到再從多個候選節點保底
        let reportId =
            filterFd.get('ReportId') ||
            document.querySelector('[name="ReportId"]')?.value ||
            document.querySelector('#reportSelect')?.value ||
            document.querySelector('#reportId')?.value ||
            '';

        // 沒選報表就不做了
        const def = reportId ? ReportConfig.get(reportId) : null;
        if (!def) {
            alert('請先選擇要建立的報表種類');
            return;
        }

        const fd = new FormData();

        // ⭐ 只有需要時間的報表才收集時間
        if (def.timePolicy !== 'none') {
            const timeFd = ReportTime.collect('#reportControls');
            const start = timeFd.get('Start');
            const end = timeFd.get('End');

            // 有填才檢查先後；允許空白（由後端套預設或報錯）
            if (start && end && new Date(start) > new Date(end)) {
                alert('時間資訊設定錯誤：開始時間不可大於結束時間');
                return;
            }
            for (const [k, v] of timeFd.entries()) fd.append(k, v);
        }

        // 把篩選項目丟進去
        for (const [k, v] of filterFd.entries()) fd.append(k, v);

        // ⭐ 確保 ReportId 一定存在（for 我的最愛序列化）
        if (!fd.get('ReportId') && reportId) fd.append('ReportId', reportId);

        // 標題 & 圖表型別
        const title = (document.querySelector('#cardTitleInput')?.value || '').trim();
        const typeSel = document.querySelector('#globalReportType');
        const chartType = typeSel?.value || def.defaultType || 'bar';

        return {
            title: title || def.title || '未命名卡片',
            chartType,
            reportId,
            base: def.base,
            endpoint: def.endpoint,
            formData: fd,
        };
    }

    // === UI：面板顯示 / 隱藏 ===
    function showPanel({ forEdit = false, cardState = null } = {}) {
        if (!forEdit) {
            // 新增模式：限制數量
            const snapshot = [];
            CardBus.forEach(({ state }) => snapshot.push(state));
            if (snapshot.length >= 6) {
                alert("一個畫面最多只能有6張卡片");
                return;
            }
        }

        const panel = document.querySelector('#reportControls');
        panel.hidden = false;
        document.querySelector('#btnConfirmAdd').hidden = forEdit;
        document.querySelector('#btnConfirmEdit').hidden = !forEdit;

        const titleInput = document.querySelector('#cardTitleInput');
        const typeSel = document.querySelector('#globalReportType'); // ⭐ 圖表型別選擇器
        const reportSel =
            document.querySelector('#globalReportId') ||
            document.querySelector('#reportSelect') ||
            document.querySelector('#reportId') ||
            document.querySelector('[name="ReportId"]');

        if (forEdit && cardState) {
            // 名稱
            titleInput.value = cardState.title || '';

            // 報表種類：選回 + 觸發重建篩選 UI
            if (reportSel) {
                reportSel.value = cardState.reportId || '';
                reportSel.dispatchEvent(new Event('change'));
            }

            // ⭐ 圖表型別：回填
            queueMicrotask(() => {
                const typeSel = document.querySelector('#globalReportType');
                if (typeSel) typeSel.value = cardState.chartType || 'bar';
            });


            // ⭐ 時間：回填
            const pick = (k) => cardState.form?.[k] ?? '';
            ReportTime.set({
                TimeUnit: pick('TimeUnit'),
                Start: pick('Start'),
                End: pick('End')
            }, '#reportControls'); // 會自動切換 block 並把值塞回去

            // ⭐ 篩選：等篩選 UI 渲染好後回填
            queueMicrotask(async () => {
                await ReportConfig.setFilters({
                    reportId: cardState.reportId,
                    form: cardState.form,
                    filterPanel: '#reportFilterPanel'
                });
            });
        } else {
            // 新增模式：清空所有可見欄位
            if (titleInput) titleInput.value = '';
            if (typeSel) typeSel.value = 'bar'; // 或你想要的預設
            // 清空時間/篩選（不帶舊值）：維持目前 UI 所在的報表 id，不做 set() 回填
            // 如果想要更乾淨，也可在此把 #globalReportId 切回第一個選項並重新 render()
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
            //alert('找不到報表設定（base/endpoint）。請確認報表定義是否已註冊。');
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
        const snapshot = [];
        CardBus.forEach(({ state }) => snapshot.push(state));

        if (snapshot.length > 6) {
            alert("最多只能存6張卡片");
            return;
        }
        if (snapshot.length <= 0) {
            alert("最少要有1張卡片");
            return;
        }

        const name = prompt('請為「我的最愛」命名：');
        if (!name) {
            alert("請提供我的最愛名稱。");
            return;
        } 
        
        const ok = confirm(`將以「${name}」儲存目前頁面 ${snapshot.length} 張卡片，確認？`);
        if (!ok) return;

        let result = await ReportConfig._fetchPostJSON('/ReportForm/ReportForm/FavoritesSave', {
            name,
            cards: snapshot
        }); // 你可在後端定義接收 DTO，將 cards[] 存 JSON。 :contentReference[oaicite:12]{index = 12}

        if (result) {
            await refreshFavorites();
            alert('已儲存到我的最愛。');
        }
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
