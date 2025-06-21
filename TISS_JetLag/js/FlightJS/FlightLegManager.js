const FlightLegManager = (() => {
    let outboundIndex = 0;
    let returnIndex = 0;
    let cityOptionsHtml = "";

    function setCityOptions(optionsHtml) {
        cityOptionsHtml = optionsHtml;
    }

    function init(initialOutboundCount, initialReturnCount) {
        outboundIndex = initialOutboundCount;
        returnIndex = initialReturnCount;

        document.getElementById("addOutboundLegBtn")?.addEventListener("click", () => {
            addLeg("OutboundLegs", outboundIndex++, "outboundLegsContainer");
        });

        document.getElementById("addReturnLegBtn")?.addEventListener("click", () => {
            addLeg("ReturnLegs", returnIndex++, "returnLegsContainer");
        });

        document.addEventListener("click", e => {
            if (e.target.closest(".remove-leg-btn")) {
                e.target.closest(".flight-leg-block")?.remove();
            }
        });
    }

    function addLeg(prefix, index, containerId) {
        const container = document.getElementById(containerId);
        const allLegs = container.querySelectorAll(".flight-leg-block");

        let lastArrivalAirport = "";
        let lastArrivalTime = "";

        if (allLegs.length > 0) {
            const lastLeg = allLegs[allLegs.length - 1];
            lastArrivalAirport = lastLeg.querySelector(`select[name*='ArrivalCity']`)?.value || "";
            lastArrivalTime = lastLeg.querySelector(`input[name*='ArrivalTimeLocal']`)?.value || "";
        }

        const template = `
<div class="row g-3 mb-3 flight-leg-block border rounded p-3 bg-light">
    <div class="col-md-5">
        <label class="form-label">出發機場</label>
        <select name="${prefix}[${index}].DepartureCity" class="form-select">
            <option value="">請選擇</option>
            ${cityOptionsHtml}
        </select>
    </div>
    <div class="col-md-5">
        <label class="form-label">抵達機場</label>
        <select name="${prefix}[${index}].ArrivalCity" class="form-select">
            <option value="">請選擇</option>
            ${cityOptionsHtml}
        </select>
    </div>
    <div class="col-md-6">
        <label class="form-label">出發時間（當地）</label>
        <input type="datetime-local" name="${prefix}[${index}].DepartureTimeLocal" class="form-control" value="${lastArrivalTime}" />
    </div>
    <div class="col-md-6">
        <label class="form-label">抵達時間（當地）</label>
        <input type="datetime-local" name="${prefix}[${index}].ArrivalTimeLocal" class="form-control" />
    </div>
    <div class="col-12 text-end">
        <button type="button" class="btn btn-danger btn-sm mt-2 remove-leg-btn">
            <i class="fas fa-minus-circle"></i> 移除航段
        </button>
    </div>
</div>`;

        container.insertAdjacentHTML("beforeend", template);

        const newBlock = container.querySelectorAll(".flight-leg-block")[index];
        const depSelect = newBlock.querySelector(`select[name="${prefix}[${index}].DepartureCity"]`);
        if (depSelect) depSelect.value = lastArrivalAirport;
    }

    return { init, setCityOptions };
})();