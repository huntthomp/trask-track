<script>
    import { PUBLIC_BACKEND_HOST } from "$env/static/public";
    import { onMount } from "svelte";
    import { Plus } from "@lucide/svelte";
    import Spinner from "$lib/components/Spinner.svelte";
    import CalendarItem from "$lib/components/calendar/CalendarItem.svelte";
    import NewCalendarModal from "$lib/components/calendar/NewCalendarModal.svelte";

    let calendarsJson = "";
    let calendars = [];
    let loading = true;
    let showModal = false;
    let toast = null;
    let toastTimeout;

    async function getCalendars() {
        try {
            const response = await fetch(
                `${PUBLIC_BACKEND_HOST}/calendar/list`,
                {
                    method: "GET",
                    credentials: "include",
                    headers: { "Content-Type": "application/json" },
                },
            );

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

    async function handleAddCalendar(e) {
        const { name, url, color } = e.detail;

        try {
            const body = {
                CalendarName: name,
                CalendarIcsUrl: url,
                Metadata: {
                    Color: color,
                },
            };

            const response = await fetch(
                `${PUBLIC_BACKEND_HOST}/calendar/new`,
                {
                    method: "POST",
                    credentials: "include",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify(body),
                },
            );

            if (!response.ok) throw new Error(await response.text());
        } catch (err) {
            showToast(err.message, "error");
        } finally {
            await getCalendars();
        }
    }

    function closeToast() {
        if (!toast) return;

        toast.isClosing = true;

        setTimeout(() => {
            toast = null;
            if (toastTimeout) clearTimeout(toastTimeout);
        }, 300);
    }

    function showToast(message, type = "error") {
        toast = { message, type, isClosing: false };

        if (toastTimeout) clearTimeout(toastTimeout);

        toastTimeout = setTimeout(() => {
            closeToast();
        }, 3000);
    }

    async function handleEdit(e) {
        const { id, name, url, color } = e.detail;

        try {
            const body = {
                CalendarId: id,
                CalendarName: name,
                CalendarIcsUrl: url,
                Metadata: {
                    Color: color,
                },
            };

            const response = await fetch(
                `${PUBLIC_BACKEND_HOST}/calendar/update`,
                {
                    method: "PUT",
                    credentials: "include",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify(body),
                },
            );

            if (!response.ok) throw new Error(await response.text());
        } catch (err) {
            showToast(err.message, "error");
        } finally {
            await getCalendars();
        }
    }

    async function handleDelete(e) {
        const { id } = e.detail;

        try {
            const response = await fetch(
                `${PUBLIC_BACKEND_HOST}/calendar/${id}`,
                {
                    method: "DELETE",
                    credentials: "include",
                    headers: { "Content-Type": "application/json" },
                },
            );

            if (!response.ok) throw new Error(await response.text());
        } catch (err) {
            showToast(err.message, "error");
        } finally {
            await getCalendars();
        }
    }

    onMount(() => {
        getCalendars();
    });
</script>

<div
    class="fixed top-0 left-0 right-0 pt-[env(safe-area-inset-top)] px-[15px] pb-2 border-b border-gray-800 bg-[#090C0E]"
>
    <p class="text-3xl font-semibold">Calendars</p>
</div>
<button
    class="fixed bottom-24 right-2 bg-[#5a8759] rounded-full min-w-[60px] min-h-[60px] mx-[10px] my-[10px] flex justify-center items-center"
    on:click={() => (showModal = true)}
>
    <Plus class="w-1/2 h-1/2" />
</button>
<div class="mt-[env(safe-area-inset-top)]"></div>
<NewCalendarModal bind:isOpen={showModal} on:add={handleAddCalendar} />

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
        <p class="text-1xl text-[#374e59]">No calendars</p>
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

{#if toast}
    <div
        class="fixed top-4 left-4 right-4 mt-[env(safe-area-inset-top)] max-w-sm mx-auto bg-[#11161a] border border-[#FFAAAA] rounded-lg p-4 shadow-lg z-50"
        class:animate-slide-down={!toast.isClosing}
        class:animate-slide-up={toast.isClosing}
        role="alert"
    >
        <div class="flex items-start justify-between">
            <p class="text-sm font-medium text-red-800">{toast.message}</p>
            <button
                on:click={closeToast}
                class="text-red-400 hover:text-red-600 transition-colors"
                aria-label="Close"
            >
                ✕
            </button>
        </div>
    </div>
{/if}

<style>
    @keyframes slide-down {
        from {
            opacity: 0;
            transform: translateY(-100px);
        }
        to {
            opacity: 1;
            transform: translateY(0);
        }
    }

    @keyframes slide-up {
        from {
            opacity: 1;
            transform: translateY(0);
        }
        to {
            opacity: 0;
            transform: translateY(-100px);
        }
    }

    .animate-slide-down {
        animation: slide-down 0.3s ease-out forwards;
    }

    .animate-slide-up {
        animation: slide-up 0.3s ease-out forwards;
    }
</style>
