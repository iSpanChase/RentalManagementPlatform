(function () {
    // 命名空間
    const ns = (window.ReportTime = window.ReportTime || {});

    // 切換不同 timeUnit 的輸入區塊顯示
    function switchTimeUnit(root) {
        const unit = root.querySelector("#globalTimeUnit").value;
        // 隱藏所有
        root.querySelectorAll("#time-selectors .time-unit-input-block").forEach(x => x.hidden = true);
        // 顯示對應
        const block = root.querySelector(`#time-selectors .input-${unit}`);
        if (block) block.hidden = false;

        // 如果是週，且週的選單還沒填，把週數選單用你的函式補起來
        if (unit === "week") {
            // 你的 generateWeeksSelect 會掃描 root 節點內的 .startWeekYear/.endWeekYear 與對應 select
            // 並自動呼叫 fillWeekSelect 來填入週數:contentReference[oaicite:2]{index=2}:contentReference[oaicite:3]{index=3}
            const weekBlock = root.querySelector(".input-week");
            if (weekBlock && !weekBlock.dataset.weeksFilled) {
                generateWeeksSelect(weekBlock); // 直接套用你現成的周生成器
                weekBlock.dataset.weeksFilled = "1";
            }
        }
    }

    // 從外部控制區塊收集 TimeUnit/Start/End（沿用你的規則實作）
    // 使用你既有的 firstWeekOfYear / formatDate / formatDateNumber 規則:contentReference[oaicite:4]{index=4}
    function collectExternalTime(root) {
        const fd = new FormData();
        const timeUnit = root.querySelector("#globalTimeUnit").value;
        fd.append("TimeUnit", timeUnit);

        let start, end;

        switch (timeUnit) {
            case "day": {
                start = root.querySelector(".input-day .startDate").value;
                end = root.querySelector(".input-day .endDate").value;
                break;
            }
            case "week": {
                const sy = parseInt(root.querySelector(".input-week .startWeekYear").value);
                const sw = parseInt(root.querySelector(".input-week .startWeekWeek").value);
                const ey = parseInt(root.querySelector(".input-week .endWeekYear").value);
                const ew = parseInt(root.querySelector(".input-week .endWeekWeek").value);

                // 使用 firstWeekOfYear(...) 當年的週一作為基準，再往後加 7*(週數-1) 天，最後用 formatDate 輸出字串:contentReference[oaicite:5]{index=5}
                start = formatDate(firstWeekOfYear(sy).addDays(7 * (sw - 1)));
                end = formatDate(firstWeekOfYear(ey).addDays(7 * (ew - 1)));
                break;
            }
            case "month": {
                const sy = root.querySelector(".input-month .startMonthYear").value;
                const sm = root.querySelector(".input-month .startMonthMonth").value;
                const ey = root.querySelector(".input-month .endMonthYear").value;
                const em = root.querySelector(".input-month .endMonthMonth").value;

                // 使用 formatDateNumber(y, m, 1) 輸出 yyyy-mm-01:contentReference[oaicite:6]{index=6}
                start = formatDateNumber(sy, sm, 1);
                end = formatDateNumber(ey, em, 1);
                break;
            }
            case "quarter": {
                const sy = root.querySelector(".input-quarter .startQuarterYear").value;
                const sq = root.querySelector(".input-quarter .startQuarterQuarter").value;
                const ey = root.querySelector(".input-quarter .endQuarterYear").value;
                const eq = root.querySelector(".input-quarter .endQuarterQuarter").value;

                // 以每季第一個月(1,4,7,10) 的 1 號代表該季
                start = formatDateNumber(sy, (sq * 3) - 2, 1);
                end = formatDateNumber(ey, (eq * 3) - 2, 1);
                break;
            }
            case "year": {
                const sy = root.querySelector(".input-year .startYearInput").value;
                const ey = root.querySelector(".input-year .endYearInput").value;

                // 以每年的 1/1 作為代表日:contentReference[oaicite:7]{index=7}
                start = formatDateNumber(sy, 1, 1);
                end = formatDateNumber(ey, 1, 1);
                break;
            }
        }

        fd.append("Start", start);
        fd.append("End", end);
        return fd;
    }

    // 對外：初始化控制區塊（綁定切換、預設顯示、周選單）
    ns.init = function (rootSelector = "#reportControls") {
        const root = document.querySelector(rootSelector);
        if (!root) return;

        // 初始顯示正確的 timeUnit 區塊
        switchTimeUnit(root);

        // 監聽切換
        root.querySelector("#globalTimeUnit").addEventListener("change", () => {
            switchTimeUnit(root);
        });

        // 若一進來就是「週」，馬上填週
        if (root.querySelector("#globalTimeUnit").value === "week") {
            const weekBlock = root.querySelector(".input-week");
            if (weekBlock && !weekBlock.dataset.weeksFilled) {
                generateWeeksSelect(weekBlock); // 使用你的周選單產生器:contentReference[oaicite:8]{index=8}:contentReference[oaicite:9]{index=9}
                weekBlock.dataset.weeksFilled = "1";
            }
        }
    };

    // 對外：收集外部時間（給呼叫 API/新增卡片用）
    ns.collect = function (rootSelector = "#reportControls") {
        const root = document.querySelector(rootSelector);
        if (!root) return new FormData();
        return collectExternalTime(root);
    };

    // 追加到 report-time-selection.js 最後（ns.collect 下方）
    ns.set = function setTime({ TimeUnit, Start, End }, rootSelector = "#reportControls") {
        const root = document.querySelector(rootSelector);
        if (!root) return;

        const unitSel = root.querySelector("#globalTimeUnit");
        unitSel.value = TimeUnit || "day";
        // 觸發區塊切換與週數初始化
        const switchEvt = new Event("change");
        unitSel.dispatchEvent(switchEvt);

        // yyyy-mm-dd 轉 Date
        const toDate = (s) => {
            if (!s) return null;
            const [y, m, d] = s.split("-").map(Number);
            return new Date(y, m - 1, d);
        };

        const start = toDate(Start);
        const end = toDate(End);

        switch (unitSel.value) {
            case "day": {
                root.querySelector(".input-day .startDate").value = Start || "";
                root.querySelector(".input-day .endDate").value = End || "";
                break;
            }
            case "week": {
                const weekBlock = root.querySelector(".input-week");
                if (weekBlock && !weekBlock.dataset.weeksFilled) {
                    generateWeeksSelect(weekBlock); // 你既有的週數產生器
                    weekBlock.dataset.weeksFilled = "1";
                }
                const weekIndexOf = (date, year) => {
                    // 依你現有 firstWeekOfYear 定義反推週次
                    const base = firstWeekOfYear(year);
                    const diff = Math.floor((date - base) / (1000 * 60 * 60 * 24));
                    return Math.floor(diff / 7) + 1;
                };

                const sy = start?.getFullYear() ?? new Date().getFullYear();
                const sw = weekIndexOf(start, sy);

                const ey = end?.getFullYear() ?? sy;
                const ew = weekIndexOf(end, ey);

                weekBlock.querySelector(".startWeekYear").value = String(sy);
                fillWeekSelect(sy, weekBlock.querySelector(".startWeekWeek"));
                weekBlock.querySelector(".startWeekWeek").value = String(sw);

                weekBlock.querySelector(".endWeekYear").value = String(ey);
                fillWeekSelect(ey, weekBlock.querySelector(".endWeekWeek"));
                weekBlock.querySelector(".endWeekWeek").value = String(ew);
                break;
            }
            case "month": {
                const [sy, sm] = (Start || "").split("-").map(Number);
                const [ey, em] = (End || "").split("-").map(Number);
                if (sy) root.querySelector(".input-month .startMonthYear").value = sy;
                if (sm) root.querySelector(".input-month .startMonthMonth").value = sm;
                if (ey) root.querySelector(".input-month .endMonthYear").value = ey;
                if (em) root.querySelector(".input-month .endMonthMonth").value = em;
                break;
            }
            case "quarter": {
                const qOf = (m) => Math.floor((m - 1) / 3) + 1;
                const [sy, sm] = (Start || "").split("-").map(Number);
                const [ey, em] = (End || "").split("-").map(Number);
                if (sy) root.querySelector(".input-quarter .startQuarterYear").value = sy;
                if (sm) root.querySelector(".input-quarter .startQuarterQuarter").value = qOf(sm);
                if (ey) root.querySelector(".input-quarter .endQuarterYear").value = ey;
                if (em) root.querySelector(".input-quarter .endQuarterQuarter").value = qOf(em);
                break;
            }
            case "year": {
                const [sy] = (Start || "").split("-").map(Number);
                const [ey] = (End || "").split("-").map(Number);
                if (sy) root.querySelector(".input-year .startYearInput").value = sy;
                if (ey) root.querySelector(".input-year .endYearInput").value = ey;
                break;
            }
        }
    };

    // 自動初始化
    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", () => ns.init());
    } else {
        ns.init();
    }
})();
