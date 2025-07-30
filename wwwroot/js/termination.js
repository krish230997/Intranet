$(document).ready(function () {
    let terminations = [];
    let table = $('#proTable').DataTable({
        "pageLength": 10,
        "lengthMenu": [[10, 25, 50, 100], [10, 25, 50, 100]],
        "ordering": true,
        "paging": true,
        "info": true,
        "searching": true,
        "columns": [
            { "data": "employeeName" },
            { "data": "terminationType" },
            { "data": "noticeDate" },
            { "data": "resignDate" },
            { "data": "reason" },
            { "data": "action", "orderable": false }
        ]
    });

    function fetchTerminations() {
        $.ajax({
            url: '/Termination/GetTerminations',
            type: 'GET',
            success: function (data) {
                terminations = data;
                table.clear().rows.add(formatData(terminations)).draw();
            }
        });
    }

    function formatData(data) {
        return data.map(item => ({
            employeeName: `<img src='/${item.profilePicture}' class="rounded-circle" style="width: 40px; height: 40px;"/> ${item.employeeName}`,
            terminationType: item.terminationType,
            noticeDate: item.noticeDate,
            resignDate: item.resignDate,
            reason: item.reason,
            action: `
                <a href='#' class='editTermination' data-id='${item.terminationId}'><i class='ti ti-edit'></i></a>
                <a href='#' class='deleteTermination' data-id='${item.terminationId}'><i class='bi bi-trash'></i></a>
            `
        }));
    }

    $(document).on("keyup", "#terminationSearch", function () {
        table.search(this.value).draw();
    });

    $(document).on("click", ".sort-option", function () {
        let sortOrder = $(this).data("sort");
        table.order([0, sortOrder]).draw();
    });

    $("#dateFilter").on("change", function () {
        let selectedFilter = $(this).val();
        filterTerminationsByDate(selectedFilter);
    });

    function filterTerminationsByDate(filterOption) {
        let today = moment().startOf("day");
        let startDate, endDate;

        if (filterOption === "today") {
            startDate = today;
            endDate = today.clone().endOf("day");
        } else if (filterOption === "tomorrow") {
            startDate = moment().add(1, "days").startOf("day");
            endDate = moment().add(1, "days").endOf("day");
        } else if (filterOption === "last7days") {
            startDate = moment().subtract(6, "days").startOf("day");
            endDate = today.clone().endOf("day");
        } else if (filterOption === "lastmonth") {
            startDate = moment().subtract(1, "months").startOf("month");
            endDate = moment().subtract(1, "months").endOf("month");
        } else {
            table.clear().rows.add(formatData(terminations)).draw();
            return;
        }

        let filteredData = terminations.filter(item => {
            let noticeDate = moment(item.noticeDate, "YYYY-MM-DD");
            return noticeDate.isBetween(startDate, endDate, null, '[]');
        });

        table.clear().rows.add(formatData(filteredData)).draw();
    }

    $(document).on("click", "#addTerminationButton", function () {
        $.get('/Termination/AddTermination', function (partialView) {
            $("#addTerminationPartial").html(partialView);
            $("#addTerminationModal").modal("show");
        });
    });

    $(document).on("submit", "#terminationForm", function (e) {
        e.preventDefault();
        $.post('/Termination/AddTermination', $(this).serialize(), function (response) {
            if (response.success) {
                alert("Termination Added Successfully!!");
                $("#addTerminationModal").modal("hide");
                fetchTerminations();
            } else {
                alert(response.message);
            }
        });
    });

    $(document).on("click", ".deleteTermination", function () {
        const id = $(this).data("id");
        if (confirm("Are you sure you want to delete this termination?")) {
            $.post('/Termination/DeleteTermination', { id }, function (response) {
                if (response.success) {
                    fetchTerminations();
                } else {
                    alert(response.message);
                }
            });
        }
    });

    $(document).on("click", ".editTermination", function () {
        const id = $(this).data("id");
        $.get(`/Termination/EditTermination/${id}`, function (partialView) {
            $("#editTerminationPartial").html(partialView);
            $("#editTerminationModal").modal("show");
        });
    });

    $(document).on("submit", "#editTerminationForm", function (e) {
        e.preventDefault();
        $.post('/Termination/EditTermination', $(this).serialize(), function (response) {
            if (response.success) {
                $("#editTerminationModal").modal("hide");
                fetchTerminations();
            } else {
                alert(response.message);
            }
        });
    });

    fetchTerminations();
});
