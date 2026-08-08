import {
	defineStore
} from 'pinia'
let initCar = {
	goodsId: 0,
	skuId: 0,
	num: 1,
	cartIds: [],
	CouponConsumeId: 0,
}
export const useOrderStore = defineStore('order', {
	state: () => {
		return {
			queryCar: {
				...initCar
			},
			address: {
				Id: ""
			},
			userArr: [],
			statusName: ['待支付', '待审核', '已发布', '进行中', '待收货', '已完成', '售后中', '售后完成', '已取消', '已驳回'],
			statusUserName: ['待支付', '待审核', '已申请', '进行中', '待确认', '已完成', '售后中', '售后完成', '已取消', '已驳回']
		}
	},  
	computed: {

	},
	actions: {
		isSelectUser(id) {
			return this.userArr.some(item => item.Id == id)
		},
		userArrDel(id) {
			let index = this.userArr.findIndex(item => item.Id == id)
			this.userArr.splice(index, 1)
		},
		userArrEdit(query) {
			let index = this.userArr.findIndex(item => item.Id == query.Id)
			if (index != -1) {
				this.userArr[index] = query
			}
		},
		initQueryCar() {
			this.queryCar = {
				...initCar
			}
		},
	},
	unistorage: true, // 是否持久化
})