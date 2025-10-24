<script>
    import { LogOut } from "@lucide/svelte";
    import { onMount } from "svelte";
    const backendApiUrl = import.meta.env.VITE_BACKEND_API_URL;

    let returnUrl = "";

    onMount(() => {
        returnUrl = `${window.location.origin}`;
    });
</script>

<a
    href={`${backendApiUrl}/account/logout?returnUrl=${encodeURIComponent(returnUrl)}`}
    class="inline-flex items-center justify-center"
>
    <LogOut
        class="min-w-[25px] h-[25px] text-[#c9d4ca] hover:text-white transition"
    />
</a>
