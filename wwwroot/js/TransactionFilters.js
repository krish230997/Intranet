$(document).ready(function () {
    var table = $('#payslipTable').DataTable({
        pageLength: 10,
        lengthMenu: [[5, 10, 25, -1], [5, 10, 25, "All"]],
        dom: 'lfrtip',
        language: {
            lengthMenu: "Show _MENU_ entries per page",
            paginate: {
                first: "First",
                last: "Last",
                next: "Next",
                previous: "Previous"
            }
        }
    });

    function applyFilters() {
        var selectedMonth = $('#filterMonth').val();
        var selectedYear = $('#filterYear').val();
        var selectedDepartment = $('#filterDepartment').val();
        var selectedDesignation = $('#filterDesignation').val();

        table.rows().every(function () {
            var row = $(this.node());
            var rowMonth = row.find('td[data-month]').attr('data-month');
            var rowYear = row.find('td[data-year]').attr('data-year');
            var rowDepartment = row.find('td[data-dept]').attr('data-dept');

            var showRow = true;

            if (selectedMonth && rowMonth !== selectedMonth) showRow = false;
            if (selectedYear && rowYear !== selectedYear) showRow = false;
            if (selectedDepartment && rowDepartment !== selectedDepartment) showRow = false;

            if (showRow) {
                row.show();
            } else {
                row.hide();
            }
        });

        table.draw();
    }

    $('#filterMonth, #filterYear, #filterDepartment, #filterDesignation').on('change', applyFilters);
});
