const FlightLegManager = (() => {
    let legIndex = 0;
    let cityOptionsHtml = "";

    function init(initialIndex) {
        legIndex = initialIndex;
        document.getElementById("addLegBtn")?.addEventListener("click", addLeg);

        document.addEventListener("click", e => {
            if (e.target.closest(".remove-leg-btn")) {
                e.target.closest(".flight-leg-block").remove();
            }
        });
    }

    function setCityOptions(optionsHtml) {
        cityOptionsHtml = optionsHtml;
    }

    function addLeg() {
        const container = document.getElementById("flightLegsContainer");
        const allLegs = container.querySelectorAll(".flight-leg-block");

        let lastArrivalAirport = "";
        let lastArrivalTime = "";

        if (allLegs.length > 0) {
            const lastLeg = allLegs[allLegs.length - 1];
            lastArrivalAirport = lastLeg.querySelector("select[name*='ArrivalCity']")?.value || "";
            lastArrivalTime = lastLeg.querySelector("input[name*='ArrivalTimeLocal']")?.value || "";
        }

        const template = `
    <div class="row g-3 mb-3 flight-leg-block border rounded p-3 bg-light">
        <div class="col-md-5">
            <label class="form-label">出發機場</label>
            <select name="FlightLegs[${legIndex}].DepartureCity" class="form-select">
                ${cityOptionsHtml}
            </select>
        </div>
        <div class="col-md-5">
            <label class="form-label">抵達機場</label>
            <select name="FlightLegs[${legIndex}].ArrivalCity" class="form-select">
                ${cityOptionsHtml}
            </select>
        </div>
        <div class="col-md-6">
            <label class="form-label">出發時間（當地）</label>
            <input type="datetime-local" name="FlightLegs[${legIndex}].DepartureTimeLocal" class="form-control" value="${lastArrivalTime}" />
        </div>
        <div class="col-md-6">
            <label class="form-label">抵達時間（當地）</label>
            <input type="datetime-local" name="FlightLegs[${legIndex}].ArrivalTimeLocal" class="form-control" />
        </div>
        <div class="col-12 text-end">
            <button type="button" class="btn btn-danger btn-sm mt-2 remove-leg-btn">
                <i class="fas fa-minus-circle"></i> 移除航段
            </button>
        </div>
    </div>`;

        container.insertAdjacentHTML("beforeend", template);

        const newBlock = container.querySelectorAll(".flight-leg-block")[legIndex];
        if (lastArrivalAirport) {
            newBlock.querySelector("select[name*='DepartureCity']").value = lastArrivalAirport;
        }

        legIndex++;
    }

    return { init, setCityOptions };
})();