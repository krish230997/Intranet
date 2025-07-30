$(document).ready(function () {
    let currentPage = 1;
    let entriesPerPage = parseInt($("#entriesPerPage").val(), 10);
    let totalRows = $("#adminData tr").length;
    let totalPages = Math.ceil(totalRows / entriesPerPage);

    // Function to update the table based on the current page and entries per page
    function updateTable() {
        const startIndex = (currentPage - 1) * entriesPerPage;
        const endIndex = startIndex + entriesPerPage;

        $("#adminData tr").hide();
        $("#adminData tr").slice(startIndex, endIndex).show();
    }

    // Function to update pagination controls
    function updatePagination() {
        const pagination = $("#adminPagination");
        pagination.empty();

        // Previous button
        pagination.append(`<li class="page-item ${currentPage === 1 ? 'disabled' : ''}">
            <a class="page-link" href="#" aria-label="Previous" id="prevPage">
                <span aria-hidden="true">&laquo;</span>
            </a>
        </li>`);

        // Page numbers
        for (let i = 1; i <= totalPages; i++) {
            pagination.append(`<li class="page-item ${i === currentPage ? 'active' : ''}">
                <a class="page-link" href="#" data-page="${i}">${i}</a>
            </li>`);
        }

        // Next button
        pagination.append(`<li class="page-item ${currentPage === totalPages ? 'disabled' : ''}">
            <a class="page-link" href="#" aria-label="Next" id="nextPage">
                <span aria-hidden="true">&raquo;</span>
            </a>
        </li>`);
    }

    // Event listener for entries per page change
    $("#entriesPerPage").change(function () {
        entriesPerPage = parseInt($(this).val(), 10);
        totalPages = Math.ceil(totalRows / entriesPerPage);
        currentPage = 1;
        updateTable();
        updatePagination();
    });

    // Event listener for pagination clicks
    $("#adminPagination").on("click", "a", function (e) {
        e.preventDefault();

        if ($(this).attr("id") === "prevPage") {
            if (currentPage > 1) {
                currentPage--;
            }
        } else if ($(this).attr("id") === "nextPage") {
            if (currentPage < totalPages) {
                currentPage++;
            }
        } else {
            currentPage = parseInt($(this).data("page"), 10);
        }

        updateTable();
        updatePagination();
    });

    // Initial setup
    updateTable();
    updatePagination();

    // Select/Deselect All Checkboxes
    $("#adminSelectAll").click(function () {
        const isChecked = $(this).is(":checked");
        $("input[type='checkbox'].adminRowCheckbox").prop("checked", isChecked);
    });

    // Handle Bulk Approve Button
    $("#adminBulkApprove").click(function () {
        handleBulkAction("Approve");
    });

    // Handle Bulk Reject Button
    $("#adminBulkReject").click(function () {
        handleBulkAction("Reject");
    });

    // Export Button
    $("#adminExportBtn").click(function () {
        window.location.href = "/AdminTimesheet/ExportTimesheets";
    });

    // Get Selected Timesheet IDs
    function getSelectedTimesheetIds() {
        const selectedIds = [];
        $("input[type='checkbox'].adminRowCheckbox:checked").each(function () {
            selectedIds.push($(this).closest("tr").data("id"));
        });
        return selectedIds;
    }

    function handleBulkAction(action) {
        const selectedIds = getSelectedTimesheetIds();
        if (selectedIds.length === 0) {
            alert(`No timesheets selected for ${action.toLowerCase()}.`);
            return;
        }

        $.ajax({
            url: `/AdminTimesheet/Bulk${action}`,
            type: "POST",
            traditional: true,
            data: { timesheetIds: selectedIds },
            success: function () {
                alert(`Selected timesheets ${action.toLowerCase()}d successfully!`);
                location.reload(); // Refresh the page
            },
            error: function (xhr, status, error) {
                alert(`Error: ${xhr.responseText || "Something went wrong!"}`);
            }
        });
    }

    $("#adminStatusFilter, #adminSearchInput, #adminProjectFilter").on("change input", function () {
        const status = $("#adminStatusFilter").val();
        const search = $("#adminSearchInput").val();
        const project = $("#adminProjectFilter").val();
        fetchFilteredTimesheets(status, search, project);
    });

    function fetchFilteredTimesheets(status = "", search = "", project = "") {
        $.ajax({
            url: "/AdminTimesheet/FilteredTimesheets",
            type: "GET",
            data: { status, search, project },
            success: function (data) {
                renderTimesheetTable(data);
            },
            error: function () {
                alert("Error fetching timesheet data.");
            }
        });
    }

    // Render Timesheet Table
    function renderTimesheetTable(data) {
        let rows = "";
        data.forEach(item => {
            let profilePicture = item.userProfilePicture
                ? `/Content/uploads/${item.userProfilePicture.split('/').pop()}` // Ensure correct URL
                : "/assets/img/users/default.jpg";
            let fullName = `${item.userFirstName || "N/A"} ${item.userLastName || ""}`.trim();

            console.log(item.status);

            let statusBadge = `<span class="badge bg-warning d-inline-flex align-items-center">${item.status}</span>`;
            if (item.status === "Approved") {
                statusBadge = `<span class="badge bg-success d-inline-flex align-items-center">${item.status}</span>`;
            } else if (item.status === "Rejected") {
                statusBadge = `<span class="badge bg-danger d-inline-flex align-items-center">${item.status}</span>`;
            }

            rows += `
            <tr data-id="${item.timesheetId}">
                <td><input type="checkbox" class="adminRowCheckbox" /></td>
                
                <td>
                    <div class="d-flex align-items-center">
                        <a href="#" class="avatar avatar-md border avatar-rounded">
                            <img src="${profilePicture}" class="img-fluid" alt="User Image">
                        </a>
                        <div class="ms-2">
                            <h6 class="fw-medium"><a href="#">${fullName}</a></h6>
                        </div>
                    </div>
                </td>

                <td>${new Date(item.date).toLocaleDateString()}</td>
                <td>${item.projectName || "N/A"}</td>
                <td>${item.workHours}</td>
                <td>${statusBadge}</td>
            </tr>`;
        });
        $("#adminData").html(rows);
        totalRows = data.length;
        totalPages = Math.ceil(totalRows / entriesPerPage);
        updateTable();
        updatePagination();
    }

    $("#exportExcelBtn").click(function () {
        const status = $("#adminStatusFilter").val();
        const search = $("#adminSearchInput").val();
        const project = $("#adminProjectFilter").val();

        const queryParams = $.param({ status, search, project });
        window.location.href = `/AdminTimesheet/ExportTimesheets?${queryParams}`;
    });

    $("#exportPdfBtn").click(function () {
        const status = $("#adminStatusFilter").val();
        const search = $("#adminSearchInput").val();
        const project = $("#adminProjectFilter").val();

        const queryParams = $.param({ status, search, project });
        window.location.href = `/AdminTimesheet/ExportTimesheetsPDF?${queryParams}`;
    });
});
