(function () {
    const ns = (window.ReportCard = window.ReportCard || {});

    // 建立一張卡
    ns.createCard = async function ({ base, endpoint, title, type, formData }) {
        const container = document.querySelector("#reportCardsRow");

        // 建立 DOM
        const col = document.createElement("div");
        col.className = "col-md-6 col-xl-4";
        col.innerHTML = `
        <div class="report-card card">
            <div class="card-body">
                <h5 class="chart-title">${title}</h5>
                <canvas class="chartCanvas"></canvas>
            </div>
        </div>
    `;
        container.appendChild(col);

        const canvas = col.querySelector(".chartCanvas");

        // 呼叫 API
        const resp = await fetch(`${base}${endpoint}`, { method: "POST", body: formData });
        if (!resp.ok) {
            alert("載入失敗");
            return;
        }
        const data = await resp.json();

        new Chart(canvas, {
            type,
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
                scales: { y: { beginAtZero: true } }
            }
        });
    };
})();
