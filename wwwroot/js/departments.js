$(document).ready(function () {
    fetchDept();
    //// Check the active view by inspecting a unique element
    //if ($('#departmentTable').length > 0) {
    //    // If department table exists, fetch department data
    //    fetchDept();
    //}

    //if ($('#designationTable').length > 0) {
    //    // If designation table exists, fetch designation data
    //    fetchDesignation();
    //}
});
$("#openmod").click(function () {
    $("#exampleModal").modal('show');
});

$("#savebtn").click(function () {
    //var obj = {
    //    name: $("#Name").val(),
    //    email: $("#Email").val(),
    //    dept: $("#Dept").val(),
    //    salary: $("#Salary").val()
    //};
    var isValid = true; // Validation flag

    // Validate required input fields
    $("#myform input[required]").each(function () {
        if ($(this).val().trim() === "") {
            $(this).addClass("is-invalid");
            // Add error message below the input if not already there
            if ($(this).next(".error-msg").length === 0) {
                $(this).after('<div class="error-msg text-danger">This field is required</div>');
            } else {
                $(this).next(".error-msg").show();
            }
            isValid = false;
        } else {
            $(this).removeClass("is-invalid");
            $(this).next(".error-msg").hide();
        }
    });

    // Validate dropdown selection (Status)
    var statusValue = $("#Status").val();
    if (!statusValue || statusValue === "") {
        $("#Status").addClass("is-invalid");
        // Add the error message below the dropdown if it's not already there
        if ($("#Status").next(".error-msg").length === 0) {
            $("#Status").after('<div class="error-msg text-danger">Please select a valid status</div>');
        } else {
            $("#Status").next(".error-msg").show(); // Show the existing error message
        }
        isValid = false;
    } else {
        $("#Status").removeClass("is-invalid");
        $("#Status").next(".error-msg").hide(); // Hide the error message
    }

    // Stop form submission if validation fails
    if (!isValid) return;

    var obj = $("#myform").serialize();

    $.ajax({
        url: '/Employee/AddNewDepartment',
        type: 'POST',
        contentType: 'application/x-www-form-urlencoded; charset=utf8',
        data: obj,
        dataType: 'json',
        success: function () {
            alert("Department Added Successfully");
            $('#exampleModal').modal('hide'); // Close the modal
            location.reload();
        },
        error: function () {
            alert("Something went wrong");
        }
    });
});
function fetchDept() {
    $.ajax({
        url: '/Employee/FetchDept',
        type: 'Get',
        contentType: 'application/json; charset=utf8',
        dataType: 'json',
        success: function (result, status, xhr) {
            console.log(result); // Add this to see the data returned
            var obj = '';
            $.each(result, function (index, item) {
                console.log(item);
                let statusBadge = '';
                if (item.status === "Active") {
                    statusBadge = '<span class="badge bg-success"><i class="ti ti-point-filled me-1"></i>' + item.status + '</span>'; // Green badge
                } else {
                    statusBadge = '<span class="badge bg-danger"><i class="ti ti-point-filled me-1"></i>' + item.status + '</span>'; // Red badge
                }
                obj += "<tr>";
                obj += "<td>" + item.departmentId + "</td>";
                obj += "<td>" + item.name + "</td>";
                obj += "<td>" + item.noOfEmployee + "</td>";
                obj += "<td>" + statusBadge + "</td>";
                obj += "<td>" + item.createdBy + "</td>";
                // obj += "<td>" + item.createdAt + "</td>";
                obj += "<td>" + item.modifiedBy + "</td>";
                //obj += "<td>" + item.modifiedAt + "</td>";
                obj += "<td>";
                obj += "<a href='#' class='edit-btn' data-id='" + item.departmentId + "'><i class='ti ti-edit'></i></a> ";
                obj += "<a href='#' class='delete-btn' data-id='" + item.departmentId + "'><i class='bi bi-trash'></i></a>";
                obj += "</td>";
                obj += "</tr>";
            });
            $("#mydata").html(obj);

            // ✅ Ensure DataTables is loaded before initializing
            if (!$.fn.DataTable) {
                console.error("DataTables library is not loaded.");
                return;
            }

            // ✅ Destroy previous DataTable instance before reinitializing
            if ($.fn.DataTable.isDataTable("#table")) {
                $("#table").DataTable().destroy();
            }

            // ✅ Initialize DataTables after data is loaded
            $("#table").DataTable({
                "paging": true,        // Enable pagination
                "pageLength": 5,       // Default rows per page
                "lengthMenu": [5,10, 25, 50, 100], // Dropdown for selecting rows per page
                "ordering": true,      // Enable column sorting
                "searching": true,     // Enable search box
                "lengthChange": true,
                "info": true,          // Show table info
                "responsive": true,    // Make table responsive
                "autoWidth": false,     // Disable auto column width


            });
            attachEventHandlers();
        },
        error: function () {
            alert("Something went wrong");
        }
    })
}
//function attachEventHandlers() {
//    $(document).on("click", ".btn-edit", function () {
//        let id = $(this).data("id");
//        fetchDepartmentDetails(id);
//        //alert("Edit clicked for ID: " + id);
//        // Add your edit logic here (e.g., open a modal for editing)
//    });

//    $(document).on("click", ".btn-delete", function () {
//        let id = $(this).data("id");
//        if (confirm("Are you sure you want to delete this record?")) {
//            deleteDepartment(id);
//            //alert("Deleted record with ID: " + id);
//            // Add delete logic (e.g., AJAX request to remove from database)
//        }
//    });
//}

//function attachEventHandlers() {
//    $(".edit-btn").click(function () {
//        var id = $(this).data("id");
//        fetchDepartmentDetails(id);         //fetch detail and show edit model
//    });

//    $(".delete-btn").click(function () {
//        var id = $(this).data("id");
//        if (confirm("Are you sure you want to delete this department?")) {
//            deleteDepartment(id);
//        }
//    });
//}
function attachEventHandlers() {
    // Use event delegation for edit buttons
    $('#table').on('click', '.edit-btn', function (e) {
        e.preventDefault(); // Prevent default action (e.g., following a link)
        var id = $(this).data("id");
        fetchDeptDetails(id); // Fetch details and show edit modal
    });

    // Use event delegation for delete buttons
    $('#table').on('click', '.delete-btn', function (e) {
        e.preventDefault(); // Prevent default action (e.g., following a link)
        var id = $(this).data("id");
        if (confirm("Are you sure you want to delete this department?")) {
            deleteDepartment(id);
        }
    });
}
function fetchDeptDetails(id) {
    console.log("Fetching role details for ID:", id);  // Debugging log

    $.ajax({
        url: '/Employee/EditDept?eid=' + id,
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            console.log("Response received:", response); // Debugging log

            if (response.success) {
                var desi = response.data;

              
                $("#DepartmentId").val(desi.departmentId);
                $("#editName").val(desi.name);

                // Populate status dropdown
                var statusDropdown = $("#editStatus");
                statusDropdown.empty();
                statusDropdown.append(`<option value="${desi.status}" selected>${desi.status}</option>`);

                $.each(response.statuses, function (i, status) {
                    if (status !== desi.status) {
                        statusDropdown.append(`<option value="${status}">${status}</option>`);
                    }
                });

                $("#editModal").modal("show");
            } else {
                console.error("Error:", response.message);
                alert(response.message);
            }
        },
        error: function (xhr, status, error) {
            console.error("AJAX Error:", status, error); // Log AJAX errors
            alert("Failed to fetch role details.");
        }
    });
}




// Save Edited Department
$("#saveEdit").click(function () {
    var obj = $("#editForm").serialize();
    $.ajax({
        url: '/Employee/EditDepartment',
        type: 'POST',
        data: obj,
        success: function () {
            alert("Department updated successfully");
            $("#editModal").modal("hide");
            fetchDept();
            location.reload();
        },
        error: function () {
            alert("Failed to update department");
        }
    });
});

// Delete Department
//function deleteDepartment(id) {
//    $.ajax({
//        url: '/Employee/DeleteDepartment',
//        type: 'POST',
//        data: { id: id },
//        success: function () {
//            alert("Department deleted successfully");
//            //$("#deleteModal").modal("hide");
//            fetchDept();
//            location.reload();
//        },
//        error: function () {
//            alert("Failed to delete department");
//        }
//    });
//}

function deleteDepartment(id) {
    $.ajax({
        url: '/Employee/DeleteDepartment',
        type: 'POST',
        data: { id: id },
        success: function () {
            alert("Department deleted successfully");
            //$("#deleteModal").modal("hide");
            fetchDept();
            location.reload();
        },
        error: function (xhr, status, error) {
            var errorMessage = xhr.responseText ? JSON.parse(xhr.responseText).message : "An unexpected error occurred.";
            alert("Error: " + errorMessage);
        }
    });
}

$("#opt").on('click', function () {
    var data = $("#opt").val();
    $.ajax({
        url: '/Employee/Sorting?mydata=' + data,
        type: 'Get',
        contentType: 'application/json; charset=utf8',
        dataType: 'json',
        success: function (result, status, xhr) {
            console.log(result); // Add this to see the data returned
            var obj = '';
            $.each(result, function (index, item) {
                console.log(item);
                let statusBadge = '';
                if (item.status === "Active") {
                    statusBadge = '<span class="badge bg-success"><i class="ti ti-point-filled me-1"></i>' + item.status + '</span>'; // Green badge
                } else {
                    statusBadge = '<span class="badge bg-danger"><i class="ti ti-point-filled me-1"></i>' + item.status + '</span>'; // Red badge
                }
                obj += "<tr>";
                obj += "<td>" + item.departmentId + "</td>";
                obj += "<td>" + item.name + "</td>";
                obj += "<td>" + item.noOfEmployee + "</td>";
                obj += "<td>" + statusBadge + "</td>";
                obj += "<td>" + item.createdBy + "</td>";
                // obj += "<td>" + item.createdAt + "</td>";
                obj += "<td>" + item.modifiedBy + "</td>";
                //obj += "<td>" + item.modifiedAt + "</td>";
                obj += "<td>";
                obj += "<a href='#' class='edit-btn' data-id='" + item.departmentId + "'><i class='ti ti-edit'></i></a> ";
                obj += "<a href='#' class='delete-btn' data-id='" + item.departmentId + "'><i class='bi bi-trash'></i></a>";
                obj += "</td>";
                obj += "</tr>";
            });
            $("#mydata").html(obj);
        },
        error: function () {
            alert("Something went wrong");
        }
    })
})


$("#sts").on('change', function () {
    var data = $(this).val(); // Get the selected value from the dropdown
    $.ajax({
        url: '/Employee/SortingStatus?mydata=' + data, // Call the SortingStatus endpoint
        type: 'GET',
        contentType: 'application/json; charset=utf8',
        dataType: 'json',
        success: function (result) {
            var obj = '';
            $.each(result, function (index, item) {

                console.log(item);
                let statusBadge = '';
                if (item.status === "Active") {
                    statusBadge = '<span class="badge bg-success"><i class="ti ti-point-filled me-1"></i>' + item.status + '</span>'; // Green badge
                } else {
                    statusBadge = '<span class="badge bg-danger"><i class="ti ti-point-filled me-1"></i>' + item.status + '</span>'; // Red badge
                }
                obj += "<tr>";
                obj += "<td>" + item.departmentId + "</td>";
                obj += "<td>" + item.name + "</td>";
                obj += "<td>" + item.noOfEmployee + "</td>";
                obj += "<td>" + statusBadge + "</td>";
                obj += "<td>" + item.createdBy + "</td>";
                // obj += "<td>" + item.createdAt + "</td>";
                obj += "<td>" + item.modifiedBy + "</td>";
                //obj += "<td>" + item.modifiedAt + "</td>";
                obj += "<td>";
                obj += "<a href='#' class='edit-btn' data-id='" + item.departmentId + "'><i class='ti ti-edit'></i></a> ";
                obj += "<a href='#' class='delete-btn' data-id='" + item.departmentId + "'><i class='bi bi-trash'></i></a>";
                obj += "</td>";
                obj += "</tr>";
            });
            $("#mydata").html(obj); // Update the table body with the filtered data
            attachEventHandlers();  // Reattach event handlers for edit and delete buttons
        },
        error: function () {
            alert("Failed to filter departments by status.");
        }
    });
});


///search query
$("#inp").on('input', function () {
    var data = $("#inp").val(); // Get the value from the search box
    $.ajax({
        url: '/Employee/SearchDept?mydata=' + data, // Update with your search API endpoint
        type: 'GET', // Use GET to fetch search results
        contentType: 'application/json; charset=utf8', // Set the content type
        dataType: 'json', // Expect JSON response
        success: function (result, status, xhr) {
            console.log(result); // Add this to see the data returned
            var obj = '';
            $.each(result, function (index, item) {
                console.log(item);
                let statusBadge = '';
                if (item.status === "Active") {
                    statusBadge = '<span class="badge bg-success"><i class="ti ti-point-filled me-1"></i>' + item.status + '</span>'; // Green badge
                } else {
                    statusBadge = '<span class="badge bg-danger"><i class="ti ti-point-filled me-1"></i>' + item.status + '</span>'; // Red badge
                }
                obj += "<tr>";
                obj += "<td>" + item.departmentId + "</td>";
                obj += "<td>" + item.name + "</td>";
                obj += "<td>" + item.noOfEmployee + "</td>";
                obj += "<td>" + statusBadge + "</td>";
                obj += "<td>" + item.createdBy + "</td>";
                // obj += "<td>" + item.createdAt + "</td>";
                obj += "<td>" + item.modifiedBy + "</td>";
                //obj += "<td>" + item.modifiedAt + "</td>";
                obj += "<td>";
                obj += "<a href='#' class='edit-btn' data-id='" + item.departmentId + "'><i class='ti ti-edit'></i></a> ";
                obj += "<a href='#' class='delete-btn' data-id='" + item.departmentId + "'><i class='bi bi-trash'></i></a>";
                obj += "</td>";
                obj += "</tr>";
            });
            $("#mydata").html(obj); // Replace the table body with the new rows
        },
        error: function () {
            alert("Something went wrong"); // Alert on error
        }
    });
});

//$(document).ready(function () {
//    $('#rows').on('change', function () {
//        var selectedRows = $(this).val();

//        if (selectedRows !== "0") {
//            $.ajax({
//                url: '/Employee/GetDepartments', // Update to your controller action
//                type: 'GET',
//                data: { rows: selectedRows },
//                success: function (data) {
//                    var tableBody = $('#departmentTable tbody');
//                    tableBody.empty(); // Clear existing rows

//                    // Populate the table with the returned data
//                    data.forEach(function (department) {
//                        var row = `<tr>
//                            <td>${department.id}</td>
//                            <td>${department.name}</td>
//                            <td>${department.status}</td>
//                        </tr>`;
//                        tableBody.append(row);
//                    });
//                },
//                error: function (error) {
//                    console.error("Error fetching data:", error);
//                }
//            });
//        }
//    });
//});


$("#exportBtn").click(function () {
    $("#exportModal").modal('show');
});

$("#exportToPDF").click(function () {
    const { jsPDF } = window.jspdf;
    const doc = new jsPDF();

    // Get the table headers
    var headers = [];
    $('#mydata').closest('table').find('thead th').each(function () {
        headers.push($(this).text());
    });

    // Get the table rows
    var rows = [];
    $('#mydata tr').each(function () {
        var row = [];
        $(this).find('td').each(function () {
            row.push($(this).text());
        });
        if (row.length > 0) {
            rows.push(row);
        }
    });

    // Generate the table in the PDF
    doc.autoTable({
        head: [headers], // Headers
        body: rows,      // Data rows
    });

    // Save the PDF
    doc.save('departments.pdf');
});

//$("#exportToExcel").click(function () {
//    var wb = XLSX.utils.book_new();
//    var ws = XLSX.utils.table_to_sheet($('#mydata')[0]);
//    //var ws = XLSX.utils.table_to_sheet($('#departmentTable')[0]); // Make sure '#departmentTable' is the correct table ID

//    // Add sheet to the workbook
//    XLSX.utils.book_append_sheet(wb, ws, "Departments");

//    // Download the Excel file
//    XLSX.writeFile(wb, 'departments.xlsx');
//});
$("#exportToExcel").click(function () {
    // Store the last column elements
    var lastTh = $('#table th:last-child').detach();
    var lastTds = $('#table td:last-child').detach();

    var wb = XLSX.utils.book_new();
    var ws = XLSX.utils.table_to_sheet($('#table')[0]);

    // Add sheet to the workbook
    XLSX.utils.book_append_sheet(wb, ws, "Department");

    // Download the Excel file
    XLSX.writeFile(wb, 'Departments.xlsx');

    // Restore the last column
    $('#table thead tr').append(lastTh);
    $('#table tbody tr').each(function (index, row) {
        $(row).append(lastTds[index]);
    });
});