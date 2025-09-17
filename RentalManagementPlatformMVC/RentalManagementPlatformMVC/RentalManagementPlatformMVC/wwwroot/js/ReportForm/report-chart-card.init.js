(function () {
    const ns = (window.ReportCard = window.ReportCard || {});
    let _seq = 1;

    ns.createCard = async function ({ base, endpoint, title, type, formData }) {
        const container = document.querySelector("#reportCardsRow");

        const cardId = `rc_${_seq++}`;
        const col = document.createElement("div");
        col.className = "col-md-6 col-xl-4";
        col.innerHTML = `
            <div class="report-card card" data-card-id="${cardId}">
                <div class="card-body position-relative">
                    <div class="d-flex justify-content-between align-items-start">
                        <h5 class="chart-title mb-2">${title}</h5>
                        <div class="btn-group btn-group-sm">
                            <button class="btn btn-outline-warning btn-edit" title="編輯">✎</button>
                            <button class="btn btn-outline-danger btn-delete" title="刪除">❌</button>
                        </div>
                    </div>
                    <canvas class="chartCanvas"></canvas>
                    <div class="divCanvas"></div>
                </div>
            </div>
        `;
        container.appendChild(col);

        const state = {
            title,
            chartType: type,
            base,
            endpoint,
            reportId: formData.get('ReportId') || '',
            // 方便「我的最愛」序列化：把 FormData 攤平成物件（載入時再重建）
            form: Object.fromEntries(formData.entries())
        };

        async function requestAndRender() {
            const resp = await fetch(`${state.base}${state.endpoint}`, {
                method: "POST", body: (() => {
                    const fd = new FormData();
                    for (const [k, v] of Object.entries(state.form)) fd.append(k, v);
                    return fd;
                })()
            });
            if (!resp.ok) throw new Error("載入失敗");

            const data = await resp.json();

            if (state.chartType == "metric") {
                const chartCanvas = col.querySelector(".chartCanvas");
                chartCanvas.style.display = "none";
                const divCanvas = col.querySelector(".divCanvas");
                divCanvas.style.display = "";

                divCanvas.innerHTML = `<h1>${data.data} </h1>`;

                return new Chart(chartCanvas, {
                    type: state.chartType,
                    data: {
                        labels: [],
                        datasets: []
                    },
                    options: {
                        responsive: true,
                        plugins: { legend: { display: false } },
                        scales: {} // 不建立任何座標軸
                    }
                });;
            }
            else {
                const chartCanvas = col.querySelector(".chartCanvas");
                chartCanvas.style.display = "";
                const divCanvas = col.querySelector(".divCanvas");
                divCanvas.style.display = "none";

                return new Chart(chartCanvas, {
                    type: state.chartType,
                    data: {
                        labels: data.labels,
                        datasets: [{
                            label: data.label,
                            data: data.data,
                            borderWidth: 1,
                            backgroundColor: data.colors
                        }]
                    },
                    options: {
                        responsive: true,
                        scales: { y: { beginAtZero: true } },
                        plugins: { legend: { display: false } }
                    }
                });
            }
        }

        let chart = await requestAndRender();

        // 編輯/刪除事件
        col.querySelector('.btn-edit').addEventListener('click', () => {
            window.__enterEditCard && window.__enterEditCard(cardId);
        });
        col.querySelector('.btn-delete').addEventListener('click', () => {
            window.__deleteCard && window.__deleteCard(cardId);
        });

        // 提供更新方法（給編輯用）
        async function update(newState) {
            // 合併新 state
            state.title = newState.title;
            state.chartType = newState.chartType;
            state.base = newState.base;
            state.endpoint = newState.endpoint;
            state.reportId = newState.reportId;

            // ⭐ 用最新面板資料覆蓋整份 form，並確保 ReportId 一定有
            state.form = Object.fromEntries(newState.formData.entries());
            if (state.reportId)
                state.form['ReportId'] = state.reportId;

            // 更新標題
            col.querySelector('.chart-title').textContent = state.title;

            // 依新的 base/endpoint + form 重新取數重繪
            chart.destroy();
            chart = await requestAndRender();
        }

        return { id: cardId, el: col, chart, state, update };
    };
})();
