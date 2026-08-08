<template>
	<view>
		<up-radio-group v-model="defalueValue" @change="groupChange">
			<view class="square" v-for="item, index in addressList" :key="index">
				<view class="top" @click="selectAddress(item)">
					<view class="title">
						<view class="name">{{ item.consignee }}</view>
						<view class="phone">{{ item.mobile }}</view>
					</view>
					<view class="address">
						{{ item.province + ' ' + item.city + ' ' + item.area + ' ' + item.address }}
					</view>
				</view>
				<view class="down">
					<view class="radio">
						<up-radio label="默认地址" :name="item.Id" activeColor="#E30013;" />
					</view>
					<view class="right">
						<view class="edit" @click="toEdit(item)">
							<up-icon name="edit-pen" size="34rpx"></up-icon>
							<view class="text">编辑</view>
						</view>
						<view class="del" @click="clickDel(item, index)">
							<up-icon name="trash" size="34rpx"></up-icon>
							<view class="text">删除</view>
						</view>
					</view>
				</view>
			</view>
		</up-radio-group>
		<view class="empty" v-if="addressList.length <= 0">
			<image src="/static/img32.png" mode="widthFix"></image>
		</view>
		<view style="height: 116rpx;"></view>
		<view class="btns">
			<view class="btn" @click="toEdit()">新增地址</view>
		</view>
	</view>
</template>

<script>
export default {
	data() {
		return {
			defalueValue: -1,
			addressList: [],
			query: {
				pageNum: 1,
				pageSize: 10,
			},
			total: 0,
			select: false
		};
	},
	onLoad(options) {
		if (options.select) {
			this.select = options.select
		}
	},
	onShow() {
		this.query.pageNum = 1
		this.getAddressList()
	},
	async onReachBottom() {
		if (this.total > this.query.pageNum * this.query.pageSize) {
			this.query.pageNum += 1
			await this.getAddressList()
		}
	},
	methods: {
		async getAddressList(reset) {
			if (reset) this.query.pageNum = 1
			let res = await this.request('User/AddressList', this.query)
			if (this.query.pageNum == 1) {
				this.addressList = res.data.items
				if (this.addressList.length != 0 && this.addressList[0].isDefault == 1) {
					this.defalueValue = this.addressList[0].Id
				}
				this.total = res.data.total
			} else {
				this.addressList.push(...res.data.items)
			}
		},
		selectAddress(item) {
			if (this.select) {
				this.storeOrder.address = item
				uni.navigateBack()
				return
			}
			this.toEdit(item)
		},
		groupChange(e) {
			this.request('User/AddressDefault', {
				queryId: e
			}).then(res => {
				this.toast('设置成功')
			})
		},
		toEdit(item) {
			this.href(`/pagesPerson/address/edit?item=${item ? JSON.stringify(item) : ''}`)
		},
		clickDel(item, index) {
			this.modal("删除地址", `您确定要删除${item.address}吗?`, callback => {
				if (callback) {
					this.request('User/DelUserAddress', {
						ids: [item.Id]
					}).then(res => {
						this.toast('删除成功')
						this.addressList.splice(index, 1)
						this.getAddressList(true)
					})
				}
			})
		}
	}
}
</script>

<style lang="scss" scoped>
.square {
	padding: 0 32rpx;
	background: #fff;
	width: 100%;
	padding-top: 32rpx;

	.top {
		padding-bottom: 32rpx;
		border-bottom: 1px solid #F5F5F5;

		.title {
			display: flex;
			align-items: center;

			.name {
				font-weight: 600;
				font-size: 30rpx;
				color: #000000;
			}

			.phone {
				margin-left: 30rpx;
			}
		}

		.address {
			font-size: 26rpx;
			color: #9F9F9F;
			padding-top: 20rpx;
		}
	}

	.down {
		padding-top: 20rpx;
		display: flex;
		justify-content: space-between;
		align-items: center;

		.radio {}

		.right {
			display: flex;
			align-items: center;

			>view {
				display: flex;
				align-items: center;

				.text {
					margin-left: 15rpx;
					font-size: 26rpx;
					color: #9F9F9F;
				}
			}

			.del {
				margin-left: 63rpx;
			}
		}
	}
}

.btns {
	padding: 15rpx 32rpx;
	width: 100%;
	position: fixed;
	left: 0;
	bottom: 24rpx;
	box-shadow: 0rpx 0rpx 16rpx 1rpx rgba(12, 106, 106, 0.1);
	background: #FFFFFF;

	.btn {
		width: 100%;
		text-align: center;
		padding: 20rpx 0;
		font-size: 28rpx;
		color: #FFFFFF;
		background: #8C4FFF;
		border-radius: 35rpx;
	}
}
</style>