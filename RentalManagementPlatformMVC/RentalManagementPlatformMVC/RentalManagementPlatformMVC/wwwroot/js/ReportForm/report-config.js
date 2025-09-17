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

            const timeWrap = document.querySelector('.time-area');
            if (timeWrap) timeWrap.hidden = (def?.timePolicy === 'none');

            const chartType = document.querySelector('.chart-type');
            if (chartType) chartType.hidden = (def?.cardKind === 'metric');

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


    // 小工具：安全抓 JSON + 填充 select
    ns._fetchGetJSON = async function _fetchGetJSON(url) {
        const resp = await fetch(url, {
            method: 'GET'
        });
        if (!resp.ok) throw new Error(`Fetch failed: ${resp.status}`);
        return await resp.json();
    }

    ns._fetchPostJSON = async function _fetchPostJSON(url, data = null) {
        try {
            const options = {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' }
            };
            if (data !== null) options.body = JSON.stringify(data);

            const resp = await fetch(url, options);
            if (!resp.ok) {
                const errorText = await resp.text();
                throw new Error(errorText);
            }
            return await resp.json();
        } catch (err) {
            alert(err.message);
        }
    };

    ns._fillSelect = function _fillSelect(select, items, {
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

    ns._fillCheckbox = function _fillCheckbox(filled, items, className) {
        filled.innerHTML = ''; // 先清空，避免重複
        for (const it of items) {
            const opt = document.createElement("div");
            opt.classList.add("form-check");
            opt.innerHTML = `
      <input class="form-check-input ${className}" type="checkbox" value="${it.id}" id="${className}-${it.id}">
      <label class="form-check-label" for="${className}-${it.id}">${it.name}</label>
    `;
            filled.appendChild(opt);
        }
    };

    // 回填篩選
    ns.setFilters = async function ({ reportId, form, filterPanel = '#reportFilterPanel' } = {}) {
        const panel = document.querySelector(filterPanel);
        if (!reportId || !panel) return;
        const def = _registry.get(reportId);
        if (def?.fillFilters) {
            await def.fillFilters(panel, form); // 交給各報表自行回填（可處理非同步）
        }
    };

})();
