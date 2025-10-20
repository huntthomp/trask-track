<script>
    import { Edit2, Trash2 } from "@lucide/svelte";
    import { createEventDispatcher } from "svelte";

    const dispatch = createEventDispatcher();

    export let data;
    export let isEditing = false;
    export let editName = "";
    export let editColor = "";

    $: syncTime = formatSyncTime(data.syncedAt);

    function formatSyncTime(dateString) {
        if (!dateString || dateString === "0001-01-01T00:00:00") {
            return "Never synced";
        }
        const date = new Date(dateString);
        const now = new Date();
        const diff = now - date;
        const seconds = Math.floor(diff / 1000);
        const minutes = Math.floor(seconds / 60);
        const hours = Math.floor(minutes / 60);
        const days = Math.floor(hours / 24);

        if (seconds < 60) return "just now";
        if (minutes < 60) return `${minutes} min${minutes > 1 ? "s" : ""} ago`;
        if (hours < 24) return `${hours} hour${hours > 1 ? "s" : ""} ago`;
        if (days < 7) return `${days} day${days > 1 ? "s" : ""} ago`;

        return date.toLocaleDateString();
    }

    function handleEdit() {
        isEditing = true;
        editName = data.calendarName;
        editColor = data.metadata.Color;
        dispatch("editStart");
    }

    function handleDelete() {
        dispatch("delete");
    }

    function handleEditSave() {
        isEditing = false;
        dispatch("editSave", { name: editName, color: editColor });
    }

    function handleEditCancel() {
        isEditing = false;
        dispatch("editCancel");
    }
</script>

<div
    class="bg-gray-900 rounded-xl border border-gray-800 shadow-sm hover:shadow-md transition-shadow overflow-hidden"
>
    {#if isEditing}
        <div class="p-4 space-y-4">
            <div>
                <p class="text-xs font-medium text-gray-400 block mb-2">
                    Calendar Name
                </p>
                <input
                    type="text"
                    bind:value={editName}
                    class="w-full px-3 py-2 border border-gray-700 rounded-lg text-base bg-gray-800 text-white focus:outline-none focus:ring-2 focus:ring-[#5a8759] focus:border-transparent"
                    placeholder="Calendar name"
                />
            </div>
            <div>
                <p class="text-xs font-medium text-gray-400 block mb-2">
                    Color
                </p>
                <div class="flex gap-2">
                    <input
                        type="color"
                        bind:value={editColor}
                        class="w-12 h-10 border border-gray-700 rounded-lg cursor-pointer"
                    />
                    <input
                        type="text"
                        bind:value={editColor}
                        maxlength="7"
                        class="flex-1 px-3 py-2 border border-gray-700 rounded-lg text-sm bg-gray-800 text-white font-mono focus:outline-none focus:ring-2 focus:ring-[#5a8759] focus:border-transparent"
                        placeholder="#5a8759"
                    />
                </div>
            </div>
            <div class="flex gap-3 pt-2">
                <button
                    on:click={handleEditCancel}
                    class="flex-1 px-4 py-2 text-gray-300 border border-gray-700 rounded-lg font-medium hover:bg-gray-800 transition-colors"
                >
                    Cancel
                </button>
                <button
                    on:click={handleEditSave}
                    class="flex-1 px-4 py-2 bg-[#5a8759] text-white rounded-lg font-medium hover:opacity-90 transition-opacity"
                >
                    Save
                </button>
            </div>
        </div>
    {:else}
        <div class="p-4 flex items-center justify-between">
            <div class="flex items-center gap-3 flex-1 min-w-0">
                <div
                    class="w-3 h-3 rounded-full flex-shrink-0"
                    style={`background-color: ${data.metadata.Color}`}
                ></div>
                <div class="flex-1 min-w-0">
                    <h3 class="text-base font-medium text-white truncate">
                        {data.calendarName}
                    </h3>
                    <div class="flex items-center gap-2 mt-1">
                        <span class="text-xs text-gray-400">{syncTime}</span>
                    </div>
                </div>
            </div>
            <div class="ml-3 flex gap-1 flex-shrink-0">
                <button
                    on:click={handleEdit}
                    class="p-2 text-gray-500 hover:text-gray-300 hover:bg-gray-800 rounded-lg transition-colors"
                    aria-label="Edit calendar"
                >
                    <Edit2 size={18} />
                </button>
                <button
                    on:click={handleDelete}
                    class="p-2 text-gray-500 hover:text-red-400 hover:bg-red-950 rounded-lg transition-colors"
                    aria-label="Remove calendar"
                >
                    <Trash2 size={18} />
                </button>
            </div>
        </div>
    {/if}
</div>
