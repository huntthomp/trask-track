<script>
    import { page } from "$app/stores";
    import { CalendarPlus, User, Settings, ListChecks } from "@lucide/svelte";

    const accent = "#52b350";

    const navItems = [
        { icon: CalendarPlus, label: "Calendars", href: "/calendars" },
        { icon: ListChecks, label: "Tasks", href: "/tasks" },
        { icon: User, label: "Profile", href: "/profile" },
    ];

    $: isActive = (href) => {
        return $page.url.pathname === href;
    };
</script>

<nav
    class="fixed bottom-0 left-0 right-0 flex justify-around items-center bg-[#090C0E] border-t border-gray-800 px-0 py-2 shadow-lg shadow-black/30 pb-[env(safe-area-inset-bottom)]"
>
    {#each navItems as item}
        <a
            href={item.href}
            class="flex flex-col items-center justify-center px-3 py-2 text-gray-400 transition-colors duration-300 flex-1 hover:text-gray-200"
            style={isActive(item.href) ? `color: ${accent};` : ""}
            aria-label={item.label}
        >
            <svelte:component this={item.icon} size={28} strokeWidth={1.5} />
            <span class="text-xs mt-1 whitespace-nowrap max-sm:hidden"
                >{item.label}</span
            >
        </a>
    {/each}
</nav>

<style>
    :global(html, body) {
        height: 100%;
    }

    :global(body) {
        padding-bottom: 80px;
        display: flex;
        flex-direction: column;
    }
</style>
