<script>
    import { onMount } from "svelte";
    const backendApiUrl = import.meta.env.VITE_BACKEND_API_URL;
    let returnUrl;

    onMount(() => {
        const origin = window.location.origin;
        returnUrl = `${origin}/profile`;
    });
</script>

<a
    href={`${backendApiUrl}/account/login?returnUrl=${encodeURIComponent(returnUrl)}`}
    class="w-1/2 flex flex-row items-center justify-center border-[2px] rounded-full border-[#42a047]"
>
    <p class="text-[#c9d4ca] text-xl font-semibold">Login</p>
</a>
