<script>
    import { createEventDispatcher } from "svelte";
    import { X } from "@lucide/svelte";

    const background = "#090C0E";

    const dispatch = createEventDispatcher();

    export let isOpen = false;

    const COLORS = [
        { name: "Blue", hex: "#3b82f6" },
        { name: "Purple", hex: "#a855f7" },
        { name: "Green", hex: "#22c55e" },
        { name: "Red", hex: "#ef4444" },
        { name: "Orange", hex: "#f97316" },
        { name: "Pink", hex: "#ec4899" },
        { name: "Indigo", hex: "#6366f1" },
        { name: "Cyan", hex: "#06b6d4" },
    ];

    let calendarName = "";
    let calendarUrl = "";
    let selectedColor = COLORS[0].hex;
    let colorMode = "hex";

    const hexToRgb = (hex) => {
        const result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
        return result
            ? `${parseInt(result[1], 16)}, ${parseInt(result[2], 16)}, ${parseInt(result[3], 16)}`
            : "0, 0, 0";
    };

    const rgbToHex = (rgb) => {
        const values = rgb.match(/\d+/g);
        if (!values || values.length !== 3) return selectedColor;
        return (
            "#" +
            values
                .map((x) => {
                    const hex = parseInt(x).toString(16);
                    return hex.length === 1 ? "0" + hex : hex;
                })
                .join("")
        );
    };

    const hexToHsv = (hex) => {
        const r = parseInt(hex.slice(1, 3), 16) / 255;
        const g = parseInt(hex.slice(3, 5), 16) / 255;
        const b = parseInt(hex.slice(5, 7), 16) / 255;

        const max = Math.max(r, g, b);
        const min = Math.min(r, g, b);
        const delta = max - min;

        let h = 0;
        if (delta !== 0) {
            if (max === r) h = ((g - b) / delta + (g < b ? 6 : 0)) / 6;
            else if (max === g) h = ((b - r) / delta + 2) / 6;
            else h = ((r - g) / delta + 4) / 6;
        }

        const s = max === 0 ? 0 : delta / max;
        const v = max;

        return `${Math.round(h * 360)}, ${Math.round(s * 100)}, ${Math.round(v * 100)}`;
    };

    const hsvToHex = (hsv) => {
        const values = hsv.match(/\d+/g);
        if (!values || values.length !== 3) return selectedColor;

        const h = parseInt(values[0]) / 360;
        const s = parseInt(values[1]) / 100;
        const v = parseInt(values[2]) / 100;

        const c = v * s;
        const x = c * (1 - Math.abs(((h * 6) % 2) - 1));
        const m = v - c;

        let r, g, b;
        if (h < 1 / 6) [r, g, b] = [c, x, 0];
        else if (h < 2 / 6) [r, g, b] = [x, c, 0];
        else if (h < 3 / 6) [r, g, b] = [0, c, x];
        else if (h < 4 / 6) [r, g, b] = [0, x, c];
        else if (h < 5 / 6) [r, g, b] = [x, 0, c];
        else [r, g, b] = [c, 0, x];

        const toHex = (n) => {
            const hex = Math.round((n + m) * 255).toString(16);
            return hex.length === 1 ? "0" + hex : hex;
        };

        return `#${toHex(r)}${toHex(g)}${toHex(b)}`;
    };

    const handleColorModeChange = (rgb) => {
        if (colorMode === "rgb") {
            selectedColor = rgbToHex(rgb);
        } else if (colorMode === "hsv") {
            selectedColor = hsvToHex(rgb);
        }
    };

    const handleClose = () => {
        isOpen = false;
    };

    const handleAdd = () => {
        if (calendarName.trim()) {
            dispatch("add", {
                name: calendarName,
                url: calendarUrl,
                color: selectedColor,
            });
            calendarName = "";
            calendarUrl = "";
            selectedColor = COLORS[0].hex;
            isOpen = false;
        }
    };
</script>

<div
    class="fixed inset-0 bg-[{background}] z-50 overflow-y-auto transition-transform duration-300 ease-out flex flex-col pt-[env(safe-area-inset-top)]"
    style={isOpen ? "transform: translateY(0)" : "transform: translateY(100%)"}
>
    <div class="px-[15px] flex-1 flex flex-col">
        <div class="flex items-center justify-between mb-6">
            <h2 class="text-xl font-semibold">Add Calendar</h2>
            <button
                on:click={handleClose}
                class="p-2 hover:bg-gray-100 rounded-lg transition-colors"
                aria-label="Close modal"
            >
                <X size={20} />
            </button>
        </div>

        <div class="space-y-5 flex-1">
            <div>
                <label for="name" class="text-sm font-medium block mb-2"
                    >Calendar Name</label
                >
                <input
                    id="name"
                    type="text"
                    bind:value={calendarName}
                    placeholder="Work Tasks"
                    class="w-full px-4 py-3 border border-gray-200 rounded-lg text-base focus:outline-none focus:ring-2 focus:ring-[#5a8759] focus:border-transparent"
                />
            </div>

            <div>
                <label for="url" class="text-sm font-medium block mb-2"
                    >Calendar URL (.ics)</label
                >
                <input
                    id="url"
                    type="text"
                    bind:value={calendarUrl}
                    placeholder="https://example.com/calendar.ics"
                    class="w-full px-4 py-3 border border-gray-200 rounded-lg text-base focus:outline-none focus:ring-2 focus:ring-[#5a8759] focus:border-transparent font-mono text-sm"
                />
                <p class="text-xs text-gray-500 mt-1">
                    Paste the .ics file link here
                </p>
            </div>

            <div>
                <p class="text-sm font-medium block mb-3">Color</p>
                <div class="flex gap-4">
                    <input
                        type="color"
                        bind:value={selectedColor}
                        class="w-[150px] h-[150px] border border-gray-700 rounded-lg cursor-pointer flex-shrink-0"
                    />
                    <div class="flex-1 space-y-3">
                        <div class="flex flex-col">
                            <label for="hex" class="text-sm font-medium mb-2"
                                >Hex</label
                            >
                            <input
                                id="hex"
                                type="text"
                                value={selectedColor}
                                on:change={(e) =>
                                    (selectedColor = e.target.value)}
                                maxlength="7"
                                class="px-3 py-2 border border-gray-700 rounded-lg text-sm bg-gray-800 text-white font-mono focus:outline-none focus:ring-2 focus:ring-[#5a8759] focus:border-transparent"
                                placeholder="#5a8759"
                            />
                        </div>
                        <div class="flex flex-col">
                            <label for="rgb" class="text-sm font-medium mb-2"
                                >RGB</label
                            >
                            <input
                                id="rgb"
                                type="text"
                                value={hexToRgb(selectedColor)}
                                on:change={(e) =>
                                    handleColorModeChange(e.target.value)}
                                class="px-3 py-2 border border-gray-700 rounded-lg text-sm bg-gray-800 text-white font-mono focus:outline-none focus:ring-2 focus:ring-[#5a8759] focus:border-transparent"
                                placeholder="255, 128, 0"
                            />
                        </div>
                    </div>
                </div>
            </div>

            <div class="flex gap-3 pt-4">
                <button
                    on:click={handleClose}
                    class="flex-1 px-4 py-3 border border-gray-200 rounded-lg font-medium hover:bg-gray-50 transition-colors"
                >
                    Cancel
                </button>
                <button
                    on:click={handleAdd}
                    disabled={!calendarName.trim() || !calendarUrl.trim()}
                    class="flex-1 px-4 py-3 bg-[#5a8759] text-white rounded-lg font-medium hover:bg-blue-600 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                >
                    Add Calendar
                </button>
            </div>
        </div>
    </div>
</div>
