import {
	defineStore
} from 'pinia'
import tui from '/utils/httpRequest'
import websocketUtil from '/utils/websocket';
export const useUserStore = defineStore('user', {
	state: () => {
		return {
			user: {},
			city: '位置',
			chooseAddress: {
				province: "",
				city: "",
				area: "",
				address: "",
			},
			socket: ""
		}   
	},
	actions: {
		// 更新用户信息
		async updateUser() {
			if (tui.getToken()) {
				let res = await tui.request('User/GetUser')
				this.user = res.data
			} else {
				this.user = {}
			}
		},
		// 选择地址
		chooseRegion() {
			//获取位置
			uni.getLocation({
				type: 'gcj02',
				success: (res) => {
					uni.chooseLocation({
						latitude: res.latitude,
						longitude: res.longitude,
						success: (res2) => {
							this.chooseAddress.address = res2.name
							let addressTemp = tui.captureLocation(res2)
							this.chooseAddress.province = addressTemp.province;
							this.chooseAddress.city = addressTemp.city;
							this.chooseAddress.area = addressTemp.area;
							this.city = addressTemp.city
						}
					});
				},
				fail(error) {
					console.log('获取位置失败', error)
				}
			})
		},
		// 链接websocket
		connectSocketInit() {
			if (tui.getToken()) {
				//开启websocket
				tui.request('Chat/preConnect').then(res => {
					this.socket = new websocketUtil(
						res.data,
						3000
					);
				})
			}
		}
	},
	unistorage: true, // 是否持久化
})