import {
	useOrderStore
} from '/store/order';
import tui from './httpRequest';
import {
	useUserStore
} from '/store/user'
export default {
	data() {
		return {
			storeUser: useUserStore(),
			storeOrder: useOrderStore(),
			...tui
		}
	},
	methods: {

	},
	onShareAppMessage(res) {
		if (res.from === 'button') {
			// 来自页面内分享按钮
			console.log(res.target);
		}
		return {
			path: '/pages/index/index',
			title: '邀请您使用小程序'
			// imageUrl: 'https://cdn.uviewui.com/uview/swiper/1.jpg', // 分享图
			// desc: '小程序描述描述描述描述'
		};
	},
	onShareTimeline() {
		return {
			path: '/pages/index/index',
			title: '邀请您使用小程序'
		};
	}
}