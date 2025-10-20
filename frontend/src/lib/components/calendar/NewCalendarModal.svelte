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
            selectedColor = "blue";
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
                <div class="grid grid-cols-4 gap-3">
                    {#each COLORS as color}
                        <button
                            on:click={() => (selectedColor = color.hex)}
                            class={`w-full aspect-square rounded-lg transition-all ${
                                selectedColor === color.hex
                                    ? "ring-2 ring-offset-2 ring-gray-900 scale-105"
                                    : "hover:scale-105"
                            }`}
                            style={`background-color: ${color.hex}`}
                            title={color.name}
                            aria-label={color.name}
                        ></button>
                    {/each}
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
