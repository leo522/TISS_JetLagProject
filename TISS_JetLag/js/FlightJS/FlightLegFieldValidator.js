const FlightLegFieldValidator = (() => {
    function validateRequiredFields() {
        const allLegs = document.querySelectorAll(".flight-leg-block");
        let isValid = true;
        let errorMessages = [];

        allLegs.forEach((block, idx) => {
            const prefix = `第 ${idx + 1} 段航班`;
            const depSelect = block.querySelector("select[name*='DepartureCity']");
            const arrSelect = block.querySelector("select[name*='ArrivalCity']");
            const depTime = block.querySelector("input[name*='DepartureTimeLocal']");
            const arrTime = block.querySelector("input[name*='ArrivalTimeLocal']");

            let blockHasError = false;

            if (!depSelect || depSelect.value === "") {
                errorMessages.push(`${prefix}：請選擇出發機場`);
                blockHasError = true;
            }

            if (!arrSelect || arrSelect.value === "") {
                errorMessages.push(`${prefix}：請選擇抵達機場`);
                blockHasError = true;
            }

            if (!depTime || depTime.value === "") {
                errorMessages.push(`${prefix}：請輸入出發時間`);
                blockHasError = true;
            }

            if (!arrTime || arrTime.value === "") {
                errorMessages.push(`${prefix}：請輸入抵達時間`);
                blockHasError = true;
            }

            if (blockHasError) {
                block.classList.add("border-danger");
                isValid = false;
            } else {
                block.classList.remove("border-danger");
            }
        });

        if (!isValid) {
            Swal.fire({
                icon: 'warning',
                title: '欄位未填寫完整',
                html: errorMessages.map(msg => `<div>${msg}</div>`).join(''),
                confirmButtonText: '我知道了'
            });
        }

        return isValid;
    }

    function bindFormValidation(formSelector) {
        const form = document.querySelector(formSelector);
        if (!form) return;

        form.addEventListener("submit", function (e) {
            if (!validateRequiredFields()) {
                e.preventDefault();
            }
        });
    }

    return {
        validateRequiredFields,
        bindFormValidation
    };
})();