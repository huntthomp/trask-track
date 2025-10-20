<script>
    import { PUBLIC_BACKEND_HOST } from "$env/static/public";
    import { onMount } from "svelte";

    let returnUrl = "";

    onMount(() => {
        returnUrl = `${window.location.origin}/profile`;
    });
</script>

<a
    href={`${PUBLIC_BACKEND_HOST}/account/login?returnUrl=${encodeURIComponent(returnUrl)}`}
    class="w-1/2 flex flex-row items-center justify-center border-[2px] rounded-full border-[#42a047]"
>
    <p class="text-[#c9d4ca] text-xl font-semibold">Login</p>
</a>
