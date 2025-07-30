$(document).ready(function () {
    // Initialize DataTable
    const table = $('#proTable').DataTable({
        pageLength: 10,
        order: [],
        paging: true,
        searching: true,
        lengthMenu: [10, 25, 50, 100],
    });

    // Fetch Resignations and Populate Table
    function fetchResignations() {
        $.ajax({
            url: '/Resignation/GetResignations',
            type: 'GET',
            success: function (data) {
                table.clear().draw();
                data.forEach(item => {
                    table.row.add([
                        `<img src='/${item.ProfilePicture}' class='rounded-circle' style='width: 40px; height: 40px;'/> ${item.EmployeeName}`,
                        item.DepartmentName,
                        item.Reason,
                        item.NoticeDate,
                        item.ResignDate,
                        `<button class='btn edit-resignation' data-id='${item.ResignationId}'><i class='ti ti-edit'></i></button>
                         <button class='btn delete-resignation' data-id='${item.ResignationId}'><i class='bi bi-trash'></i></button>`
                    ]).draw(false);
                });
            }
        });
    }

    fetchResignations();

    // Add Resignation Modal
    $(document).on('click', '#btnAddResignation', function () {

        $.get('/Resignation/AddResignation')
            .done(function (data) {
                $('#modalContainer').html(data);
                $('#addResignationModal').modal('show');
            })
            .fail(function () {
                alert("Error loading the Add Resignation form.");
            });
    });

    // Edit Resignation button click
    $(document).on('click', '.edit-resignation', function () {
        const id = $(this).data('id'); // Get the resignation ID
        $.get(`/Resignation/EditResignation/${id}`, function (data) {
            $('#modalContainer').html(data);

            // Ensure Bootstrap initializes the modal correctly
            $('#editResignationModal').modal({
                backdrop: 'static',
                keyboard: false
            });

            $('#editResignationModal').modal('show');
        });
    });

    $(document).on('click', '.btn-secondary', function () {
        $('#editResignationModal').modal('hide');
    });


    // Handle saving the changes for the edited resignation
    $(document).on('click', '#saveResignationChanges', function () {
        const formData = $('#editResignationForm').serialize();

        $.ajax({
            url: '/Resignation/EditResignation',
            type: 'POST',
            data: formData,
            success: function (response) {
                
                    location.reload(); // Fallback to reload the page
                
            },
            error: function () {
                alert('An error occurred while saving changes.');
            }
        });
    });

    // Delete Resignation
    $(document).on('click', '.delete-resignation', function () {
        const id = $(this).data('id');
        if (confirm('Are you sure you want to delete this resignation?')) {
            $.post(`/Resignation/DeleteResignation/${id}`)
                .done(() => location.reload())
                .fail(() => alert("Error deleting resignation."));
        }
    });

    // Sorting
    $('.sort-option').on('click', function () {
        const sortType = $(this).data('sort');
        table.order([4, sortType]).draw();
    });

    // Date Filtering
    $('.filter-date').on('click', function () {
        const filterType = $(this).data('filter');
        const currentDate = new Date();
        let startDate, endDate;

        switch (filterType) {
            case 'today':
                startDate = endDate = new Date();
                break;
            case 'last_week':
                startDate = new Date();
                startDate.setDate(startDate.getDate() - 7);
                endDate = new Date();
                break;
            case 'last_month':
                startDate = new Date();
                startDate.setMonth(startDate.getMonth() - 1);
                endDate = new Date();
                break;
            case 'last_year':
                startDate = new Date();
                startDate.setFullYear(startDate.getFullYear() - 1);
                endDate = new Date();
                break;
            case 'all':
                startDate = endDate = null;
                break;
        }

        $.fn.dataTable.ext.search.push(function (settings, data) {
            const resignDateStr = data[4];
            if (!resignDateStr) return false;
            const resignDate = new Date(resignDateStr);
            return (!startDate || resignDate >= startDate) && (!endDate || resignDate <= endDate);
        });

        table.draw();
        $.fn.dataTable.ext.search.pop();
    });
});
