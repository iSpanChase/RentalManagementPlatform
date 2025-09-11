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

    // ========= 以下示範兩種報表 =========

    // 1) 訂單營收趨勢（折線圖），可依房型與城市過濾
    ns.register('revenueByRoom', {
        title: '訂單營收趨勢（房型/城市）',
        defaultType: 'line',
        base: '/ReportForm/ReportForm/',
        endpoint: 'GetRevenueTrend', // 你後端可對應這個 action
        buildFilterUI(container) {
            container.innerHTML = `
        <div class="filter-row">
          <label>房型：</label>
          <select class="f-roomType">
            <option value="">(全部)</option>
            <option value="single">單人房</option>
            <option value="double">雙人房</option>
            <option value="family">家庭房</option>
          </select>

          <label>城市：</label>
          <select class="f-city">
            <option value="">(全部)</option>
            <option value="taipei">台北</option>
            <option value="taoyuan">桃園</option>
            <option value="hsinchu">新竹</option>
          </select>
        </div>
      `;
        },
        serializeFilters(container, fd) {
            fd.append('RoomType', container.querySelector('.f-roomType')?.value || '');
            fd.append('City', container.querySelector('.f-city')?.value || '');
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
