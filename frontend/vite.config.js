import { sveltekit } from '@sveltejs/kit/vite';
import { defineConfig, loadEnv } from 'vite';
import tailwindcss from '@tailwindcss/vite'


export default ({ mode }) => {
    process.env = Object.assign(process.env, loadEnv(mode, process.cwd(), ''));

    return defineConfig({
        plugins: [
            sveltekit(),
            tailwindcss(),
        ],
        server: {
            port: process.env.PORT,
            watch: {
                usePolling: true
            }
        },
    });
}
