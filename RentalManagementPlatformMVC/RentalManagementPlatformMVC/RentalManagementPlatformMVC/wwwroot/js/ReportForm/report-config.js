(function () {
    const ns = (window.ReportConfig = window.ReportConfig || {});
    const _registry = new Map();

    /**
     * 註冊一個報表
     * @param {string} id - 唯一 ID
     * @param {{
     *   title: string,
     *   defaultType?: 'bar'|'line'|'pie',
     *   base: string,
     *   endpoint: string,
     *   buildFilterUI?: (container:HTMLElement)=>void,
     *   serializeFilters?: (container:HTMLElement, fd:FormData)=>void
     * }} def
     */
    ns.register = function (id, def) {
        _registry.set(id, def);
    };

    ns.get = function (id) {
        return _registry.get(id);
    };

    /**
     * 將所有註冊的報表塞到下拉，並在切換時渲染對應的篩選 UI
     */
    ns.initControls = function ({
        reportSelect = '#globalReportId',
        filterPanel = '#reportFilterPanel'
    } = {}) {
        const sel = document.querySelector(reportSelect);
        const panel = document.querySelector(filterPanel);
        if (!sel || !panel) return;

        // 產生 options
        sel.innerHTML = '';
        for (const [id, def] of _registry.entries()) {
            const opt = document.createElement('option');
            opt.value = id;
            opt.textContent = def.title;
            sel.appendChild(opt);
        }

        // 渲染當前選擇的篩選 UI
        const render = () => {
            const id = sel.value;
            const def = _registry.get(id);
            panel.innerHTML = '';
            if (def?.buildFilterUI) {
                def.buildFilterUI(panel);
            }
            // 若報表定義了預設圖表型別，就覆寫外部的圖表型別選擇器
            if (def?.defaultType) {
                const typeSel = document.querySelector('#globalReportType');
                if (typeSel) typeSel.value = def.defaultType;
            }
        };

        sel.addEventListener('change', render);
        render(); // 初次渲染
    };

    /**
     * 將目前顯示在 filterPanel 的篩選值序列化進 FormData
     */
    ns.collectFilters = function ({
        filterPanel = '#reportFilterPanel'
    } = {}) {
        const panel = document.querySelector(filterPanel);
        const fd = new FormData();
        const currentId = document.querySelector('#globalReportId')?.value;
        if (!currentId) return fd;

        const def = _registry.get(currentId);
        fd.append('ReportId', currentId);
        if (def?.serializeFilters) {
            def.serializeFilters(panel, fd);
        }
        return fd;
    };


    // 1) 訂單營收趨勢，可依城市過濾
    // 小工具：安全抓 JSON + 填充 select
    async function _fetchJSON(url) {
        const resp = await fetch(url, {
            method: 'GET'
        });
        if (!resp.ok) throw new Error(`Fetch failed: ${resp.status}`);
        return await resp.json();
    }

    function _fillSelect(select, items, {
        includeAll = true,
        allText = '(全部)',
        allValue = ''
    } = {}) {
        select.innerHTML = '';
        if (includeAll) {
            const opt = document.createElement('option');
            opt.value = allValue;
            opt.textContent = allText;
            select.appendChild(opt);
        }
        for (const it of items) {
            const opt = document.createElement('option');
            opt.value = it.id;      // 以 id 作為 value
            opt.textContent = it.name;
            select.appendChild(opt);
        }
    }

    // 1) 訂單營收趨勢（折線圖）— 縣市/鄉區連動
    ns.register('BookingRevenueTrend', {
        title: '訂單營收趨勢',
        defaultType: 'line',
        base: '/ReportForm/ReportForm/',
        endpoint: 'BookingRevenueTrend',

        buildFilterUI(container) {
            container.innerHTML = `
      <div class="filter-row">
        <label>縣/市：</label>
        <select class="f-city"></select>
        <label>鄉/區：</label>
        <select class="f-district" disabled></select>
        <label>訂單狀態：</label>
        <select class="f-status" multiple>
          <option value="Cancelled">已取消</option>
          <option value="Pending">待確認</option>
          <option value="Confirmed">已確認</option>
          <option value="Completed">已完成</option>
        </select>
      </div>
    `;

            const citySel = container.querySelector('.f-city');
            const distSel = container.querySelector('.f-district');

            // 初始：載入所有縣市
            (async () => {
                try {
                    const cities = await _fetchJSON('/ReportForm/ReportForm/Cities');
                    _fillSelect(citySel, cities, { includeAll: true, allText: '(全部)', allValue: '' });

                    // 如果預設沒有選擇城市 => 保持鄉/區 disabled
                    distSel.innerHTML = '';
                    const emptyOpt = document.createElement('option');
                    emptyOpt.value = '';
                    emptyOpt.textContent = '(先選縣/市)';
                    distSel.appendChild(emptyOpt);
                    distSel.disabled = true;
                } catch (e) {
                    console.error(e);
                    _fillSelect(citySel, [], { includeAll: true });
                    distSel.innerHTML = '<option value="">(載入失敗)</option>';
                    distSel.disabled = true;
                }
            })();

            // 當選擇城市後，動態載入鄉區
            citySel.addEventListener('change', async () => {
                const cityId = citySel.value;
                if (!cityId) {
                    // 清空並鎖住鄉區
                    distSel.innerHTML = '';
                    const tip = document.createElement('option');
                    tip.value = '';
                    tip.textContent = '(先選縣/市)';
                    distSel.appendChild(tip);
                    distSel.disabled = true;
                    return;
                }

                try {
                    distSel.disabled = true;
                    distSel.innerHTML = '<option value="">(載入中...)</option>';
                    const districts = await _fetchJSON(`/ReportForm/ReportForm/Districts?cityId=${encodeURIComponent(cityId)}`);
                    _fillSelect(distSel, districts, { includeAll: true, allText: '(全部)', allValue: '' });
                    distSel.disabled = false;
                } catch (e) {
                    console.error(e);
                    distSel.innerHTML = '<option value="">(載入失敗)</option>';
                    distSel.disabled = true;
                }
            });
        },

        // 修正：送出 id，而不是 name；而且要抓對元素
        serializeFilters(container, fd) {
            const cityId = container.querySelector('.f-city')?.value || '';
            const districtId = container.querySelector('.f-district')?.value || '';
            let status = container.querySelector('.f-status')?.selectedOptions;
            status = container.querySelector('.f-status')?.selectedOptions;
            status = Array.from(status).map(({ value }) => value);
            fd.append('CityId', cityId);
            fd.append('DistrictId', districtId);
            fd.append('Status', status);
        }
    });

    // 2) 訂單狀態占比（圓餅圖），可選要不要含取消訂單
    ns.register('bookingStatusPie', {
        title: '訂單狀態占比',
        defaultType: 'pie',
        base: '/ReportForm/ReportForm/',
        endpoint: 'GetBookingStatusPie',
        buildFilterUI(container) {
            container.innerHTML = `
        <div class="filter-row">
          <label>狀態（可多選）：</label>
          <select multiple class="f-statuses">
            <option value="confirmed">已確認</option>
            <option value="pending">待確認</option>
            <option value="checkedin">已入住</option>
            <option value="checkedout">已退房</option>
            <option value="canceled">已取消</option>
          </select>

          <label class="ml-2">
            <input type="checkbox" class="f-includeCanceled" />
            計入取消訂單
          </label>
        </div>
      `;
        },
        serializeFilters(container, fd) {
            const statuses = Array.from(container.querySelector('.f-statuses')?.selectedOptions || []).map(o => o.value);
            fd.append('Statuses', JSON.stringify(statuses));
            fd.append('IncludeCanceled', container.querySelector('.f-includeCanceled')?.checked ? 'true' : 'false');
        }
    });

})();
