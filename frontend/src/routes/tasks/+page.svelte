<script>
    import { onMount } from "svelte";
    import Spinner from "$lib/components/Spinner.svelte";
    const backendApiUrl = import.meta.env.VITE_BACKEND_API_URL;

    let calendarsJson = "";
    let calendars = [];
    let loading = true;

    async function getTasks() {
        try {
            const response = await fetch(`${backendApiUrl}/calendar/list`, {
                method: "GET",
                credentials: "include",
                headers: { "Content-Type": "application/json" },
            });

            if (!response.ok) throw new Error(await response.text());

            calendarsJson = await response.json();
            calendars = calendarsJson.map((cal) => ({
                calendarId: cal.CalendarId,
                calendarName: cal.CalendarName,
                calendarIcsUrl: cal.CalendarIcsUrl,
                syncedAt: cal.SyncedAt,
                metadata: cal.Metadata,
            }));
        } catch (err) {
            showToast(err.message, "error");
        } finally {
            loading = false;
        }
    }

    onMount(() => {
        getTasks();
    });
</script>

<div
    class="fixed top-0 left-0 right-0 pt-[env(safe-area-inset-top)] px-[15px] pb-2 border-b border-gray-800 bg-[#090C0E] h-[45px]"
>
    <p class="text-3xl font-semibold">Tasks</p>
</div>
<div class="min-h-[50px]"></div>

{#if loading}
    <div
        class="fixed top-0 left-0 right-0 bottom-0 flex justify-center items-center"
    >
        <Spinner />
    </div>
{:else if calendars.length == 0}
    <div
        class="fixed top-0 left-0 right-0 bottom-0 flex justify-center items-center pointer-events-none"
    >
        <p class="text-1xl text-[#374e59]">No Tasks</p>
    </div>
{:else}
    <div class="px-[10px] flex flex-col gap-[10px]">
        {#each calendars as calendar}
            <CalendarItem
                data={calendar}
                on:edit={handleEdit}
                on:delete={handleDelete}
            />
        {/each}
    </div>
{/if}
