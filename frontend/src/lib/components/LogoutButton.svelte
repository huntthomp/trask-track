<script>
    import { LogOut } from "@lucide/svelte";
    import { PUBLIC_BACKEND_HOST } from "$env/static/public";
    import { onMount } from "svelte";

    let returnUrl = "";

    onMount(() => {
        returnUrl = `${window.location.origin}`;
        console.log(returnUrl);
    });
</script>

<a
    href={`${PUBLIC_BACKEND_HOST}/account/logout?returnUrl=${encodeURIComponent(returnUrl)}`}
    class="inline-flex items-center justify-center"
>
    <LogOut
        class="min-w-[25px] h-[25px] text-[#c9d4ca] hover:text-white transition"
    />
</a>
