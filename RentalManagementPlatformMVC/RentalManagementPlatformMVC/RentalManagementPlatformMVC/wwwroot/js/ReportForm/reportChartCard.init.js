(function () {
    const ns = (window.ReportCard = window.ReportCard || {});

    // 切換不同 timeUnit 的輸入區塊顯示
    function switchTimeUnit(root) {
        const unit = root.querySelector('.timeUnit').value;
        root.querySelectorAll('.time-unit-input-block').forEach(x => x.hidden = true);
        const block = root.querySelector(`.input-${unit}`);
        if (block) block.hidden = false;
    }

    // 初始化「一張卡」
    ns.initOne = function (root) {
        if (!root || root.dataset.inited === '1') return;
        root.dataset.inited = '1';

        const base = root.dataset.baseAddress || '';
        const endpoint = root.dataset.endpoint || '';
        const canvas = root.querySelector('.chartCanvas');
        let chart = null;

        // 初始顯示正確的 timeUnit 區塊
        switchTimeUnit(root);
        root.querySelector('.timeUnit').addEventListener('change', () => switchTimeUnit(root));

        // 創建周資料
        generateWeeksSelect(root);

        // 載入按鈕
        root.querySelector('.btnLoad').addEventListener('click', async () => {
            const type = root.querySelector('.reportType').value;
            const fd = collectFormData(root, new FormData());
            console.log(`${base}${endpoint}`);
            const resp = await fetch(`${base}${endpoint}`, { method: 'POST', body: fd });
            if (!resp.ok){
                alert('載入失敗');
                return;
            }
            const data = await resp.json();

            if (chart) { chart.destroy(); }
            chart = new Chart(canvas, {
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
        });

        // 移除按鈕
        root.querySelector('.btnRemove').addEventListener('click', () => {
            const col = root.closest('.col-12, .col-md-6, .col-xl-4') || root;
            col.remove();
        });

        // TODO: 這裡接上你原有的 generateWeeksSelect()，但要改成只操作 root 內的 select
        // generateWeeksSelectFor(root);
    };

    // 初始化「頁面上的所有卡」
    ns.initAll = function () {
        document.querySelectorAll('.report-card').forEach(ns.initOne);
    };

    // 頁面載入就跑一次
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', ns.initAll);
    }
    else {
        ns.initAll();
    }
})();
