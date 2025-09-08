function formatDate(year, month, day) {
    let m = String(month).padStart(2, "0");
    let d = String(day).padStart(2, "0");
    return `${year}-${m}-${d}`;
}

function formDataAppendTime(data) {//data型別為FormData
    let timeUnit = $("#timeUnit").val();
    data.append("TimeUnit", timeUnit);

    let start;
    let end;
    switch (timeUnit) {
        case "day":
            start = $("#startDate").val();
            end = $("#endDate").val();
            break;
        case "month":
            start = formatDate($("#startMonthYear").val(), $("#startMonthMonth").val(), 1);
            end = formatDate($("#endMonthYear").val(), $("#endMonthMonth").val(), 1);
            break;
        case "quarter":
            start = formatDate($("#startQuarterYear").val(), $("#startQuarterQuarter").val() * 3 - 2, 1);
            end = formatDate($("#endQuarterYear").val(), $("#endQuarterQuarter").val() * 3 - 2, 1);
            break;
        case "year":
            start = formatDate($("#startYearInput").val(), 1, 1);
            end = formatDate($("#endYearInput").val(), 1, 1);
            break;

    }

    data.append("Start", start);
    data.append("End", end);

    return data;
}