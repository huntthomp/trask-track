<script>
  import { fly } from "svelte/transition";
  import LogoutButton from "$lib/components/LogoutButton.svelte";
  export let data;
  const userClaims = data.user;

  const name = userClaims[0].value;
  const imageSrc = userClaims[4].value;
</script>

<div
  class="w-full flex flex-row justify-start items-center gap-[10px] px-[15px]"
>
  <img class="h-[70px] w-[70px] rounded-full" src={imageSrc} alt={name} />
  <div class="w-full flex">
    <p class="text-[#c9d4ca] text-3xl font-medium">{name}</p>
  </div>
  <LogoutButton />
</div>
