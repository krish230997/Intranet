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
    var obj = $("#myform").serialize();

    $.ajax({
        url: '/Employee/AddDepartment',
        type: 'POST',
        contentType: 'application/x-www-form-urlencoded; charset=utf8',
        data: obj,
        dataType: 'json',
        success: function () {
            alert("Department Added Successfully");
            $('#exampleModal').modal('hide'); // Close the modal
            fetchDept();
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
                obj += "<tr>";
                obj += "<td>" + item.departmentId + "</td>";
                obj += "<td>" + item.name + "</td>";
                obj += "<td>" + item.noofemployee + "</td>"
                obj += "<td>" + item.status + "</td>"
                obj += "<td>";
                obj += "<a href='#' class='edit-btn' data-id='" + item.departmentId + "'><i class='fas fa-edit text-warning'></i></a> ";
                obj += "<a href='#' class='delete-btn' data-id='" + item.departmentId + "'><i class='fas fa-trash-alt text-danger'></i></a>";
                obj += "</td>";
                obj += "</tr>";
            });
            $("#mydata").html(obj);

            // Debug: Check if DataTables is loaded
            console.log($.fn.DataTable);

            // Destroy existing DataTable instance if it exists
            if ($.fn.DataTable.isDataTable("#table")) {
                $("#table").DataTable().destroy();
            }

            // Reinitialize DataTable after data is loaded
            $("#table").DataTable({
                "paging": true,
                "pageLength": 5,
                "lengthMenu": [5, 10, 25, 50, 100],
                "ordering": true,
                "searching": true,
                "info": true
            });

            attachEventHandlers();
        },
        error: function () {
            alert("Something went wrong");
        }
    })
}

function attachEventHandlers() {
    $(".edit-btn").click(function () {
        var id = $(this).data("id");
        fetchDepartmentDetails(id);         //fetch detail and show edit model
    });

    $(".delete-btn").click(function () {
        var id = $(this).data("id");
        if (confirm("Are you sure you want to delete this department?")) {
            deleteDepartment(id);
        }
    });
}

// Fetch Department Details for Edit
function fetchDepartmentDetails(id) {
    $.ajax({
        url: '/Employee/FetchDept',
        type: 'GET',
        dataType: 'json',
        success: function (result) {
            var dept = result.find(d => d.departmentId === id);
            if (dept) {
                $("#editDepartmentId").val(dept.departmentId);
                $("#editName").val(dept.name);
                $("#editStatus").val(dept.status);
                $("#editModal").modal("show");
            }
        },
        error: function () {
            alert("Failed to fetch department details");
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
        },
        error: function () {
            alert("Failed to update department");
        }
    });
});

// Delete Department
function deleteDepartment(id) {
    $.ajax({
        url: '/Employee/DeleteDepartment',
        type: 'POST',
        data: { id: id },
        success: function () {
            alert("Department deleted successfully");

            fetchDept();
        },
        error: function () {
            alert("Failed to delete department");
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
                obj += "<tr>";
                obj += "<td>" + item.departmentId + "</td>";
                obj += "<td>" + item.name + "</td>";
                obj += "<td>" + item.noofemployee + "</td>";
                obj += "<td>" + item.status + "</td>"
                obj += "<td>";
                obj += "<a href='#' class='edit-btn' data-id='" + item.departmentId + "'><i class='fas fa-edit text-warning'></i></a> ";
                obj += "<a href='#' class='delete-btn' data-id='" + item.departmentId + "'><i class='fas fa-trash-alt text-danger'></i></a>";
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
                obj += "<tr>";
                obj += "<td>" + item.departmentId + "</td>";
                obj += "<td>" + item.name + "</td>";
                obj += "<td>" + item.noofemployee + "</td>";
                obj += "<td>" + item.status + "</td>";

                obj += "<td>";
                obj += "<a href='#' class='edit-btn' data-id='" + item.departmentId + "'><i class='fas fa-edit text-warning'></i></a> ";
                obj += "<a href='#' class='delete-btn' data-id='" + item.departmentId + "'><i class='fas fa-trash-alt text-danger'></i></a>";
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
                obj += "<tr>";
                obj += "<td>" + item.departmentId + "</td>";
                obj += "<td>" + item.name + "</td>";
                obj += "<td>" + item.status + "</td>"
                obj += "<td>";
                obj += "<a href='#' class='edit-btn' data-id='" + item.departmentId + "'><i class='ti ti - edit'></i></a> ";
                obj += "<a href='#' class='delete-btn' data-id='" + item.departmentId + "'><i class='bi bi - trash'></i></a>";
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

$(document).ready(function () {
    $('#rows').on('change', function () {
        var selectedRows = $(this).val();

        if (selectedRows !== "0") {
            $.ajax({
                url: '/Employee/GetDepartments', // Update to your controller action
                type: 'GET',
                data: { rows: selectedRows },
                success: function (data) {
                    var tableBody = $('#departmentTable tbody');
                    tableBody.empty(); // Clear existing rows

                    // Populate the table with the returned data
                    data.forEach(function (department) {
                        var row = `<tr>
                            <td>${department.id}</td>
                            <td>${department.name}</td>
                            <td>${department.status}</td>
                        </tr>`;
                        tableBody.append(row);
                    });
                },
                error: function (error) {
                    console.error("Error fetching data:", error);
                }
            });
        }
    });
});


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

$("#exportToExcel").click(function () {
    var wb = XLSX.utils.book_new();
    var ws = XLSX.utils.table_to_sheet($('#mydata')[0]);
    //var ws = XLSX.utils.table_to_sheet($('#departmentTable')[0]); // Make sure '#departmentTable' is the correct table ID

    // Add sheet to the workbook
    XLSX.utils.book_append_sheet(wb, ws, "Departments");

    // Download the Excel file
    XLSX.writeFile(wb, 'departments.xlsx');
});


new DataTable('#example', {
    layout: {
        bottomEnd: {
            paging: {
                firstLast: false
            }
        }
    }
});