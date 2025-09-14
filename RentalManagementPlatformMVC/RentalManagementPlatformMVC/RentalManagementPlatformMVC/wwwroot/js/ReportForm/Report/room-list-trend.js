(function () {
    const ns = (window.ReportConfig = window.ReportConfig || {});

    /** 共同邏輯（城市/區域/狀態）報表的工廠 */
    function registerReportFilter({
        id,
        title,
        endpoint,
        base = '/ReportForm/ReportForm/',
        defaultType = 'bar',
    }) {
        ns.register(id, {
            title,
            defaultType,
            base,
            endpoint,

            buildFilterUI(container) {
                container.innerHTML = `
                    <div class="row filter-row">
                        <div class="col-sm-2">
                            <label class="form-label">縣/市</label>
                            <select class="form-select f-city"></select>
                        </div>
                        <div class="col-sm-2">
                            <label class="form-label">鄉/區</label>
                            <select class="form-select f-district disabled"></select>
                        </div>
                        <div class="col-sm-3">
                            <label class="form-label">每晚價格區間</label>
                            <div class="d-flex align-items-center gap-3">
                                <input class="form-control f-price-min" type="number" min="0" max="999999">
                                <span>~</span>
                                <input class="form-control f-price-max" type="number" min="0" max="999999">
                            </div>
                        </div>
                        <div class="col-sm-3">
                            <label class="form-label">平均評分區間</label>
                            <div class="d-flex align-items-center gap-3">
                                <input class="form-control f-rating-min" type="number" min="0" max="999999">
                                <span>~</span>
                                <input class="form-control f-rating-max" type="number" min="0" max="999999">
                            </div>
                        </div>
                    </div>
                `;

                const citySel = container.querySelector('.f-city');
                const distSel = container.querySelector('.f-district');

                // 載入縣市
                (async () => {
                    try {
                        const cities = await ns._fetchGetJSON('/ReportForm/ReportForm/Cities');
                        ns._fillSelect(citySel, cities, { includeAll: true, allText: '(全部)', allValue: '' });
                        distSel.innerHTML = '<option value="">(先選縣/市)</option>';
                        distSel.disabled = true;
                    } catch {
                        ns._fillSelect(citySel, [], { includeAll: true });
                        distSel.innerHTML = '<option value="">(載入失敗)</option>';
                        distSel.disabled = true;
                    }
                })();

                // 當城市改變時載入區
                citySel.addEventListener('change', async () => {
                    const cityId = citySel.value;
                    if (!cityId) {
                        distSel.innerHTML = '<option value="">(先選縣/市)</option>';
                        distSel.disabled = true;
                        return;
                    }
                    try {
                        distSel.disabled = true;
                        distSel.innerHTML = '<option value="">(載入中...)</option>';
                        const districts = await ns._fetchGetJSON(`/ReportForm/ReportForm/Districts?cityId=${encodeURIComponent(cityId)}`);
                        ns._fillSelect(distSel, districts, { includeAll: true, allText: '(全部)', allValue: '' });
                        distSel.disabled = false;
                    } catch {
                        distSel.innerHTML = '<option value="">(載入失敗)</option>';
                        distSel.disabled = true;
                    }
                });
            },

            serializeFilters(container, fd) {
                let cityId = container.querySelector('.f-city')?.value || '';
                let districtId = container.querySelector('.f-district')?.value || '';
                let priceMin = container.querySelector('.f-price-min')?.value || '';
                let priceMax = container.querySelector('.f-price-max')?.value || '';
                let ratingMin = container.querySelector('.f-rating-min')?.value || '';
                let ratingMax = container.querySelector('.f-rating-max')?.value || '';
                fd.append('CityId', cityId);
                fd.append('DistrictId', districtId);
                fd.append('PriceMin', priceMin);
                fd.append('PriceMax', priceMax);
                fd.append('RatingMin', ratingMin);
                fd.append('RatingMax', ratingMax);
            },

            fillFilters(container, form) {
                const get = (k) => form?.[k] ?? '';
                const citySel = container.querySelector('.f-city');
                const distSel = container.querySelector('.f-district');

                const priceMinEl = container.querySelector('.f-price-min');
                const priceMaxEl = container.querySelector('.f-price-max');
                const ratingMinEl = container.querySelector('.f-rating-min');
                const ratingMaxEl = container.querySelector('.f-rating-max');
                if (priceMinEl) priceMinEl.value = get('PriceMin') || '';
                if (priceMaxEl) priceMaxEl.value = get('PriceMax') || '';
                if (ratingMinEl) ratingMinEl.value = get('RatingMin') || '';
                if (ratingMaxEl) ratingMaxEl.value = get('RatingMax') || '';

                const waitForOptions = (sel) => new Promise(res => {
                    const tick = () => (sel && sel.options && sel.options.length) ? res() : setTimeout(tick, 50);
                    tick();
                });

                return (async () => {
                    await waitForOptions(citySel);
                    citySel.value = get('CityId') || '';
                    citySel.dispatchEvent(new Event('change')); // 觸發載入區

                    await waitForOptions(distSel);
                    distSel.value = get('DistrictId') || '';
                })();
            }
        });
    }

    registerReportFilter({
        id: 'RoomListAveragePriceTrend',
        title: '房源平均金額趨勢',
        endpoint: 'RoomListAveragePriceTrend',
    });

    registerReportFilter({
        id: 'RoomListAverageRatingTrend',
        title: '房源平均評分趨勢',
        endpoint: 'RoomListAverageRatingTrend',
    });

    registerReportFilter({
        id: 'RoomListCountTrend',
        title: '房源數量趨勢',
        endpoint: 'RoomListCountTrend',
    });

    registerReportFilter({
        id: 'RoomListCreateCountTrend',
        title: '房源創建數量趨勢',
        endpoint: 'RoomListCreateCountTrend',
    });
})();