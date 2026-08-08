import App from './App'
import {
	createSSRApp
} from 'vue'
import * as Pinia from 'pinia';
import {
	createUnistorage
} from './uni_modules/pinia-plugin-unistorage'
import uviewPlus from '@/uni_modules/uview-plus'
import common from '/utils/common.js'
import {
	useUserStore
} from '/store/user'; 
export function createApp() {
	const app = createSSRApp(App)
	// 状态管理
	const store = Pinia.createPinia()
	// 持久化 
	store.use(createUnistorage())
	app.use(store)
	app.use(uviewPlus)
	app.mixin(common)
	// let storeUser = useUserStore()
	// storeUser.connectSocketInit()
	return {
		app,
		Pinia, // 此处必须将 Pinia 返回
	}
}