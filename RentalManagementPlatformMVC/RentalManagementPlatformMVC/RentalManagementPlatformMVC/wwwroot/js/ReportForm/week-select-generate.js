function generateWeeksSelect(root) {
    // 初始化
    root.querySelector(".startWeekYear").addEventListener("change", function () {
        fillWeekSelect(this.value, root.querySelector(".startWeekWeek"));
    });
    root.querySelector(".endWeekYear").addEventListener("change", function () {
        fillWeekSelect(this.value, root.querySelector(".endWeekWeek"));
    });

    // 預設先填當前年份
    fillWeekSelect(root.querySelector(".startWeekYear").value, root.querySelector(".startWeekWeek"));
    fillWeekSelect(root.querySelector(".endWeekYear").value, root.querySelector(".endWeekWeek"));
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
    select.options.length = 0;
    weeks.forEach(w => {
        let newOption = document.createElement('option');
        newOption.value = w.value;
        newOption.textContent = w.text;
        select.appendChild(newOption);
    });
}


