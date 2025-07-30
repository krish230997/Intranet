//$(document).ready(function () {
//    // Select/Deselect All Checkboxes
//    $("#selectAllEmployees").click(function () {
//        const isChecked = $(this).is(":checked");
//        $(".employeeRowCheckbox").prop("checked", isChecked);
//    });

//    // Get Selected Employee IDs
//    function getSelectedEmployeeIds() {
//        let selectedIds = [];
//        $(".employeeRowCheckbox:checked").each(function () {
//            let id = parseInt($(this).data("id"), 10); // Convert to integer
//            if (!isNaN(id)) selectedIds.push(id);
//        });
//        console.log("Selected Employee IDs:", selectedIds); // Debugging
//        return selectedIds;
//    }

//    // Handle Bulk Payslip Generation
//    function handleBulkPayslipGeneration() {
//        let selectedIds = getSelectedEmployeeIds();

//        if (selectedIds.length === 0) {
//            alert("No employees selected for payslip generation.");
//            return;
//        }

//        console.log("Sending Employee IDs:", selectedIds); // Debugging

//        $.ajax({
//            url: "/EmpSalaries/GeneratePayslipsForSelectedUsers",
//            type: "POST",
//            contentType: "application/json",
//            data: JSON.stringify(selectedIds),
//            dataType: "json",
//            success: function (response) {
//                alert("Payslips generated successfully for selected employees!");
//                location.reload(); // Refresh the page
//            },
//            error: function (xhr, status, error) {
//                console.error("AJAX Error:", xhr.responseText || error);
//                alert(`Error: ${xhr.responseText || "Something went wrong!"}`);
//            }
//        });
//    }
//    // Attach the bulk action to a button
//    $("#generatePayslipsButton").click(handleBulkPayslipGeneration);
//});

$(document).ready(function () {
    // Select/Deselect All Checkboxes
    $("#selectAllEmployees").click(function () {
        $(".employeeRowCheckbox").prop("checked", $(this).is(":checked"));
    });

    // Get Selected Employee IDs
    function getSelectedEmployeeIds() {
        let selectedIds = $(".employeeRowCheckbox:checked").map(function () {
            return parseInt($(this).data("id"), 10);
        }).get(); // Convert jQuery object to array

        console.log("Selected Employee IDs:", selectedIds);
        return selectedIds;
    }

    // Handle Bulk Payslip Generation
    function handleBulkPayslipGeneration() {
        let selectedIds = getSelectedEmployeeIds();

        if (selectedIds.length === 0) {
            alert("Please select at least one employee.");
            return;
        }

        console.log("Sending Employee IDs:", selectedIds);

        $.ajax({
            url: "/EmpSalaries/GeneratePayslipsForSelectedUsers",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(selectedIds),
            dataType: "json",
            beforeSend: function () {
                $("#generatePayslipsButton").prop("disabled", true).text("Generating...");
            },
            success: function (response) {
                console.log("Response:", response);

                if (response.failedUsers && response.failedUsers.length > 0) {
                    alert(`Payslips generated with some failures. Failed Users: ${response.failedUsers.join(", ")}`);
                } else {
                    alert("Payslips generated successfully for selected employees!");
                }

                location.reload();
            },
            error: function (xhr) {
                console.error("AJAX Error:", xhr.responseText);
                alert(`Error: ${xhr.responseJSON?.message || "Something went wrong!"}`);
            },
            complete: function () {
                $("#generatePayslipsButton").prop("disabled", false).text("Generate Payslips");
            }
        });
    }

    // Attach the bulk action to a button
    $("#generatePayslipsButton").click(handleBulkPayslipGeneration);
});
