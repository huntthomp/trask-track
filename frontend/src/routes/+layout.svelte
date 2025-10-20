<script>
  import { page } from "$app/stores";
  import BottomNav from "$lib/components/BottomNav.svelte";
  import "../app.css";
</script>

<main
  class="flex-1 pb-[env(safe-area-inset-bottom)] pt-[env(safe-area-inset-top)] min-h-lvh"
>
  <slot />
</main>

{#if $page.url.pathname !== "/"}
  <BottomNav />
{/if}
