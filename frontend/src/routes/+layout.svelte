<script>
    import { page } from "$app/stores";
    import BottomNav from "$lib/components/BottomNav.svelte";
    import "../app.css";

    const noNavRoutes = ["/", "/login"];
</script>

<main
    class="flex-1 pb-[env(safe-area-inset-bottom)] pt-[env(safe-area-inset-top)] min-h-lvh"
>
    <slot />
</main>

{#if !noNavRoutes.some((route) => $page.url.pathname === route || $page.url.pathname.startsWith(`${route}/`))}
    <BottomNav />
{/if}
