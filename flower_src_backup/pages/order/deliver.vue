<template>
	<view>
		<view class="form">
			<up-form labelPosition="top" label-width="700rpx">
				<up-form-item label="花店选择的收货方式" required>
					<up-input v-model="data.receivingTypeName" disabled></up-input>
				</up-form-item>
				<up-form-item label="收货地址" required>
					{{ data.shopProvince + data.shopCity + data.shopArea + data.shopAddress }}
				</up-form-item>
				<up-form-item label="发货类型" required>
					<up-radio-group v-model="query.deliveryType">
						<up-radio label="物流" :name="0"></up-radio>
						<up-radio label="自提" :name="1"></up-radio>
					</up-radio-group>
				</up-form-item>
				<view v-if="query.deliveryType == 0">
					<up-form-item label="物流公司" required>
						<up-input v-model="query.expressName" placeholder="请输入物流公司"></up-input>
					</up-form-item>
					<up-form-item label="物流单号" required>
						<up-input v-model="query.logisticsNo" placeholder="请输入物流单号"></up-input>
					</up-form-item>
				</view>
			</up-form>
			<view class="btns">
				<view class="btn" @click="$u.throttle(submit, 1000)">确认提交</view>
			</view>
		</view>
	</view>
</template>

<script>
export default {
	data() {
		return {
			id: '',
			query: {
				deliveryType: 0,
				expressName: '',
				logisticsNo: '',
			},
			data: {
				goodsImgList: []
			},
			info: {
				price: 0
			},
		}
	},
	onLoad(options) {
		this.id = options.id
		this.OrderDetail()
	},
	methods: {
		async OrderDetail() {
			let res = await this.request('Order/TaskDetails', {
				queryId: this.id
			})
			this.data = res.data
			this.info = res.data.info
		},
		// 提交
		submit() {
			let queryTemp = {
				...this.query,
				orderNo: this.info.orderNo,
			}
			this.modal('提示', '确定提交吗？', callback => {
				if (callback) {
					this.request('Order/TaskDelivery ', queryTemp).then(res => {
						this.toast("操作成功")
						setTimeout(()=>{
							this.back()
						},400)
					})
				}
			})
		},
	}
}
</script>

<style lang="scss" scoped>
.form {
	background: white;
	padding: 0 32rpx;
	width: 686rpx;
	margin: 32rpx;
	border-radius: 16rpx;
}

.btns {
	z-index: 99;
	padding: 15rpx 32rpx;
	width: 100%;
	position: fixed;
	left: 0;
	bottom: 0;
	box-shadow: 0rpx 0rpx 16rpx 1rpx rgba(12, 106, 106, 0.1);
	background: #FFFFFF;

	.btn {
		width: 100%;
		text-align: center;
		padding: 20rpx 0;
		font-size: 28rpx;
		color: #FFFFFF;
		background: #8C4FFF;
		border-radius: 16rpx;
	}
}
</style>
