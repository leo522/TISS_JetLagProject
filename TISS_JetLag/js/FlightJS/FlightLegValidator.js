const FlightLegValidator = (() => {
    function validateForm() {
        const allLegs = document.querySelectorAll(".flight-leg-block");
        let isValid = true;
        let errorMessages = [];

        allLegs.forEach((block, idx) => {
            const depInput = block.querySelector("input[name*='DepartureTimeLocal']");
            const arrInput = block.querySelector("input[name*='ArrivalTimeLocal']");

            if (depInput && arrInput && depInput.value && arrInput.value) {
                const depTime = new Date(depInput.value);
                const arrTime = new Date(arrInput.value);

                if (depTime >= arrTime) {
                    isValid = false;
                    errorMessages.push(`第 ${idx + 1} 段航班：出發時間不可晚於或等於抵達時間`);
                    block.classList.add("border-danger");
                } else {
                    block.classList.remove("border-danger");
                }
            }
        });

        if (!isValid) {
            Swal.fire({
                icon: 'error',
                title: '航段時間錯誤',
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
            if (!validateForm()) {
                e.preventDefault(); // 阻止表單送出
            }
        });
    }

    return {
        validateForm,
        bindFormValidation
    };
})();