function generateWeeksSelect() {
    // 初始化
    $("#startWeekYear").on("change", function () {
        fillWeekSelect($(this).val(), $("#startWeekWeek"));
    });
    $("#endWeekYear").on("change", function () {
        fillWeekSelect($(this).val(), $("#endWeekWeek"));
    });

    // 預設先填當前年份
    fillWeekSelect($("#startWeekYear").val(), $("#startWeekWeek"));
    fillWeekSelect($("#endWeekYear").val(), $("#endWeekWeek"));
}

function generateWeeksByYear(year) {
    year = parseInt(year);

    let start = firstWeekOfYear(year);

    let weeks = [];
    let weekIndex = 1;

    //一直生成週，直到超過該年的 12/31
    while (true) {
        let end = new Date(start);
        end.setDate(start.getDate() + 6); // 週日 = 週一 + 6

        // 格式化日期字串
        let startStr = `${start.getFullYear()}/${start.getMonth() + 1}/${start.getDate()}(${["日", "一", "二", "三", "四", "五", "六"][start.getDay()]})`;
        let endStr = `${end.getFullYear()}/${end.getMonth() + 1}/${end.getDate()}(${["日", "一", "二", "三", "四", "五", "六"][end.getDay()]})`;

        weeks.push({
            value: weekIndex,
            text: `第${weekIndex}週 ${startStr}~${endStr}`
        });

        // 如果這一週的週日已經在下一年，結束迴圈
        if (end.getFullYear() > year) break;

        // 下一週
        start.setDate(start.getDate() + 7);
        weekIndex++;
    }

    return weeks;
}

// 填充到 select
function fillWeekSelect(year, select) {
    let weeks = generateWeeksByYear(year);
    select.empty();
    weeks.forEach(w => {
        select.append(new Option(w.text, w.value));
    });

}



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
//data型別為FormData
function formDataAppendTime(data) {
    let timeUnit = $("#timeUnit").val();
    data.append("TimeUnit", timeUnit);

    let start;
    let end;
    switch (timeUnit) {
        case "day":
            start = $("#startDate").val();
            end = $("#endDate").val();
            break;
        case "week":
            start = formatDate(firstWeekOfYear(parseInt($("#startWeekYear").val())).addDays(7 * ($("#startWeekWeek").val() - 1)));
            end = formatDate(firstWeekOfYear(parseInt($("#endWeekYear").val())).addDays(7 * ($("#endWeekWeek").val() - 1)));
            break;
        case "month":
            start = formatDateNumber($("#startMonthYear").val(), $("#startMonthMonth").val(), 1);
            end = formatDateNumber($("#endMonthYear").val(), $("#endMonthMonth").val(), 1);
            break;
        case "quarter":
            start = formatDateNumber($("#startQuarterYear").val(), $("#startQuarterQuarter").val() * 3 - 2, 1);
            end = formatDateNumber($("#endQuarterYear").val(), $("#endQuarterQuarter").val() * 3 - 2, 1);
            break;
        case "year":
            start = formatDateNumber($("#startYearInput").val(), 1, 1);
            end = formatDateNumber($("#endYearInput").val(), 1, 1);
            break;

    }

    data.append("Start", start);
    data.append("End", end);

    return data;
}