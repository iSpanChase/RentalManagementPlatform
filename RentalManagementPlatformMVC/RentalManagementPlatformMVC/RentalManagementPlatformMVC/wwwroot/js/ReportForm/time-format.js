Date.prototype.addDays = function (days) {
    this.setDate(this.getDate() + days);
    return this;
}

//計算傳入的年份的第一周的起始日期，2025年第一周圍2024/12/30(一)
//參數型別為Int
function firstWeekOfYear(year) {
    // 先找出該年的 1/1
    let jan1 = new Date(year, 0, 1);

    // 找到包含 1/1 的那一週的「週一」
    let day = jan1.getDay(); // 0=日,1=一,...6=六
    let diff = (day === 0 ? -6 : 1 - day); // 算出需要往前推幾天，讓它變成週一
    let start = new Date(jan1);
    start.setDate(jan1.getDate() + diff);

    return start;
}

//將傳入日期格式化為字串yyyy-mm-dd
//參數型別皆為Int
function formatDateNumber(year, month, day) {
    let m = String(month).padStart(2, "0");
    let d = String(day).padStart(2, "0");
    return `${year}-${m}-${d}`;
}

//將傳入日期格式化為字串yyyy-mm-dd
//參數型別為Date
function formatDate(date) {
    let y = date.getFullYear();
    let m = String((date.getMonth() + 1)).padStart(2, "0");
    let d = String(date.getDate()).padStart(2, "0");
    return `${y}-${m}-${d}`;
}

//為傳入的FormData添加使用者填入的時間資訊
// 取得一張卡的欄位值 → 封裝你的 formDataAppendTime() 邏輯
//data型別為FormData
function collectFormData(root, formData) {
    let timeUnit = root.querySelector(".timeUnit").value;
    formData.append("TimeUnit", timeUnit);

    let start;
    let end;
    switch (timeUnit) {
        case "day":
            start = root.querySelector(".startDate").value;
            end = root.querySelector(".endDate").value;
            break;
        case "week":
            start = formatDate(firstWeekOfYear(parseInt(root.querySelector(".startWeekYear").value)).addDays(7 * (root.querySelector(".startWeekWeek").value - 1)));
            end = formatDate(firstWeekOfYear(parseInt(root.querySelector(".endWeekYear").value)).addDays(7 * (root.querySelector(".endWeekWeek").value - 1)));
            break;
        case "month":
            start = formatDateNumber(root.querySelector(".startMonthYear").value, root.querySelector(".startMonthMonth").value, 1);
            end = formatDateNumber(root.querySelector(".endMonthYear").value, root.querySelector(".endMonthMonth").value, 1);
            break;
        case "quarter":
            start = formatDateNumber(root.querySelector(".startQuarterYear").value, root.querySelector(".startQuarterQuarter").value * 3 - 2, 1);
            end = formatDateNumber(root.querySelector(".endQuarterYear").value, root.querySelector(".endQuarterQuarter").value * 3 - 2, 1);
            break;
        case "year":
            start = formatDateNumber(root.querySelector(".startYearInput").value, 1, 1);
            end = formatDateNumber(root.querySelector(".endYearInput").value, 1, 1);
            break;
    }
    formData.append("Start", start);
    formData.append("End", end);

    return formData;
}