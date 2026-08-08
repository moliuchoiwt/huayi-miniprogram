<template>
	<view>
		<view class="form">
			<up-form labelPosition="left" label-width="140rpx">
				<up-form-item label="名称" borderBottom>
					<up-input v-model="query.consignee" placeholder="请填写名称" border="none"></up-input>
				</up-form-item>
				<up-form-item label="电话" borderBottom>
					<up-input v-model="query.mobile" type="number" placeholder="请填写电话" border="none"></up-input>
				</up-form-item>
				<up-form-item label="地址" borderBottom>
					<view class="item" @click="chooseRegion">
						<view :style="{'color': address? '': '#999'}">
							{{ address || '请选择地址' }}
						</view>
						<up-icon name="arrow-right"></up-icon>
					</view>  
				</up-form-item>
				<up-form-item label="详细地址" labelPosition='top' borderBottom>
					<up-textarea v-model="query.address" placeholder="请填写详细地址" border="none"></up-textarea>
				</up-form-item>  
			</up-form>
			<view class="getAddressBtn" @click="getAddress">一键获取微信收货地址</view>
		</view>
		<view class="btns">
			<view class="btn" @click="submit">确认提交</view>
		</view>
	</view>
</template>

<script>
	import {  
		test
	} from '@/uni_modules/uview-plus';
	export default {
		data() {
			return {
				intelligentRecognition: "",
				query: {
					consignee: "",
					mobile: "",
					province: "",
					city: "",
					area: "",
					address: "",
					isDefault: 0 //1.默认
				}
			}
		},
		computed: {
			address() {
				let item = this.query
				if (!item.province) return ''
				return item.province + ' ' + item.city + ' ' + item.area + ' ' + item.address
			}
		},
		onLoad(options) {
			if (options.item) {
				this.query = JSON.parse(options.item)
			}
		},
		methods: {
			getAddress() {
				this.chooseAddress().then(resAddress => {
					this.query.consignee = resAddress.userName
					this.query.mobile = resAddress.telNumber
					this.query.province = resAddress.provinceName
					this.query.city = resAddress.cityName
					this.query.area = resAddress.countyName
					this.query.address = resAddress.detailInfo
				});
			},
			chooseAddress() {
				return new Promise((resolve, reject) => {
					uni.chooseAddress({
						success: (res) => {
							resolve(res)
						},
						fail: (err) => {
							reject(err)
						}
					})
				})
			},
			// 选择地址
			chooseRegion() {
				this.getAuthorize(() => {
					this.storeUser.chooseAddress = this.query
					this.storeUser.chooseRegion()
				})
			},
			// 提交
			submit() {
				if (!test.mobile(this.query.mobile)) {
					this.toast('手机号错误')
					return
				}
				if (!this.query.consignee) {
					this.toast('请选择地址')
					return
				}
				this.request('User/EditAddress', this.query).then(res => {
					this.toast('操作成功')
					setTimeout(() => {
						uni.navigateBack()
					}, 400)
				})
			},
		}
	}
</script>

<style lang="scss" scoped>
	.btns {
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
			background: #8C4FFF ;
			border-radius: 16rpx;
		}
	}

	.form {
		background: white;
		padding: 32rpx;
	}

	.item {
		width: 100%;
		display: flex;
		justify-content: space-between;
		padding-right: 32rpx;
	}

	.getAddressBtn {
		margin-top: 24rpx;
		width: 100%;
		text-align: center;
		padding: 20rpx 0;
		font-size: 28rpx;
		color: #FFFFFF;
		background: #8C4FFF ;
		border-radius: 16rpx;
	}
</style>